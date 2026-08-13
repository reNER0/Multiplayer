using System;
using System.Linq;
using Assets.Scripts.Network;
using UnityEngine;

public class PhysicsObject : Predictable
{
    public Rigidbody Rigidbody;
    public Transform serverStateTransform;

    protected RigidbodyState[] RigidbodyStates => LocalStates as RigidbodyState[];


    protected override void Start()
    {
        base.Start();

        OnShowServerStates(NetworkSettings.ShowServerStates);
        NetworkBus.OnShowServerStates += OnShowServerStates;

        if (!NetworkRepository.Current.IsCurrentClientOwnerOfObject(this))
        {
            // TODO : Fix this. Commented because of object owner is setting too late
            //Destroy(serverStateTransform.gameObject);
            return;
        }

        serverStateTransform.parent = null;
    }

    private void OnShowServerStates(bool show) 
    {
        serverStateTransform.gameObject.SetActive(show);
    }

    private void OnDestroy()
    {
        NetworkBus.OnShowServerStates -= OnShowServerStates;
    }

    public override void Input(PlayerInputs playerInputs)
    {
        inputSeam = true;
    }

    public override void ApplyState(PredictableState state)
    {
        var rigidbodyState = (RigidbodyState)state;

        Rigidbody.MovePosition(rigidbodyState.Position);
        Rigidbody.MoveRotation(rigidbodyState.Rotation);
        Rigidbody.velocity = rigidbodyState.Velocity;
        Rigidbody.angularVelocity = rigidbodyState.RotationVelocity;
    }

    public override PredictableState GetState()
    {
        return new RigidbodyState(InputProcessor.ProcessTick,
            Rigidbody.position,
            Rigidbody.velocity,
            Rigidbody.rotation,
            Rigidbody.angularVelocity
            );
    }


    public override void SaveCurrentState(int tick)
    {
        var state = new RigidbodyState(tick,
            Rigidbody.position,
            Rigidbody.velocity,
            Rigidbody.rotation,
            Rigidbody.angularVelocity
            );

        LocalStates[tick % 1024] = state;
        LocalInputs[tick % 1024] = lastAppliedInputs; 
    }

    /// <summary>
    /// Calculates a temporary state for targetTick from the two nearest real
    /// server snapshots. The calculated state is not stored in ServerStates.
    /// </summary>
    protected T GetServerStateAtTick<T>(int targetTick) where T : RigidbodyState
    {
        T previousState = null;
        T nextState = null;

        foreach (var rawState in ServerStates)
        {
            var state = rawState as T;
            if (state == null)
                continue;

            if (state.Tick <= targetTick &&
                (previousState == null || state.Tick > previousState.Tick))
            {
                previousState = state;
            }

            if (state.Tick >= targetTick &&
                (nextState == null || state.Tick < nextState.Tick))
            {
                nextState = state;
            }
        }

        // Interpolation needs a real snapshot on both sides of targetTick.
        if (previousState == null || nextState == null)
            return null;

        var interpolation = previousState.Tick == nextState.Tick
            ? 0f
            : Mathf.InverseLerp(previousState.Tick, nextState.Tick, targetTick);

        return InterpolateServerStates(previousState, nextState, targetTick, interpolation) as T;
    }

    private static RigidbodyState InterpolateServerStates(
        RigidbodyState previousState,
        RigidbodyState nextState,
        int targetTick,
        float interpolation)
    {
        var previousPlayerState = previousState as PlayerSyncState;
        var nextPlayerState = nextState as PlayerSyncState;

        if (previousPlayerState != null && nextPlayerState != null)
        {
            return new PlayerSyncState(
                targetTick,
                Vector3.Lerp(previousPlayerState.Position, nextPlayerState.Position, interpolation),
                Vector3.Lerp(previousPlayerState.Velocity, nextPlayerState.Velocity, interpolation),
                Quaternion.Slerp(previousPlayerState.Rotation, nextPlayerState.Rotation, interpolation),
                Vector3.Lerp(previousPlayerState.RotationVelocity, nextPlayerState.RotationVelocity, interpolation),
                interpolation < 1f ? previousPlayerState.Health : nextPlayerState.Health,
                Mathf.LerpAngle(previousPlayerState.Yaw, nextPlayerState.Yaw, interpolation),
                Mathf.LerpAngle(previousPlayerState.Pitch, nextPlayerState.Pitch, interpolation));
        }

        return new RigidbodyState(
            targetTick,
            Vector3.Lerp(previousState.Position, nextState.Position, interpolation),
            Vector3.Lerp(previousState.Velocity, nextState.Velocity, interpolation),
            Quaternion.Slerp(previousState.Rotation, nextState.Rotation, interpolation),
            Vector3.Lerp(previousState.RotationVelocity, nextState.RotationVelocity, interpolation));
    }

    protected virtual void FixedUpdate()
    {
        //var serverState = lastServerState as RigidbodyState;
        var interpolateTick = NetworkTime.CurrentTick - Mathf.RoundToInt(NetworkSettings.MaximumPingInTicks * 2f);
        var serverState = GetServerStateAtTick<RigidbodyState>(interpolateTick);

        if (serverState == null)
        {
            //Debug.LogError($"Error while applying server predictable state! {interpolateTick}");
            return;
        }

        serverStateTransform.position = serverState.Position;
        serverStateTransform.rotation = serverState.Rotation;

        var localState = LocalStates.FirstOrDefault(x => x?.Tick == serverState.Tick);

        if (localState == null)
        {
            //Debug.LogWarning($"Client received server state with tick {serverState.Tick}, " +
            //    $"but clients last state tick was {States.Where(x => x != null)?.OrderByDescending(x => x.Tick).First().Tick}");
            return;
        }

        var error = (serverState.Position - (localState as RigidbodyState).Position).magnitude;
        var angularError = Quaternion.Angle(serverState.Rotation, (localState as RigidbodyState).Rotation);

        if (error >= NetworkSettings.MaximumError || angularError > 60)
        {
            Reconcilate(serverState);
            return;
        }

        if (NetworkRepository.Current.IsCurrentClientOwnerOfObject(this))
        {
            SmoothSync(localState as RigidbodyState, serverState, NetworkSettings.ClientSidePredictionType);
            return;
        }

        SmoothSync(localState as RigidbodyState, serverState, NetworkSettings.ErrorCorrectionType);
    }

    protected void SmoothSync(RigidbodyState localState, RigidbodyState serverState, ErrorCorrectionType errorCorrectionType)
    {
        if (errorCorrectionType == ErrorCorrectionType.HardSync)
        {
            Rigidbody.MovePosition(serverState.Position);
            Rigidbody.MoveRotation(serverState.Rotation);
            Rigidbody.velocity = serverState.Velocity;
            Rigidbody.angularVelocity = serverState.RotationVelocity;
            return;
        }

        var positionDelta = GetDeltaPosition(serverState, localState, errorCorrectionType);
        var rotationDelta = GetDeltaRotation(serverState, localState, errorCorrectionType);

        var velocityDelta = serverState.Velocity - localState.Velocity;
        var angularVelocityDelta = serverState.RotationVelocity - localState.RotationVelocity;

        var ticksToSmooth = Math.Max(NetworkTime.CurrentTick - serverState.Tick, 1);

        positionDelta /= ticksToSmooth;
        rotationDelta = Quaternion.Slerp(Quaternion.identity, rotationDelta, 1f / ticksToSmooth);

        var interpolationValue = (1f / ticksToSmooth) * NetworkSettings.SyncForce;

        var newPosition = GetCorrectedPosition(positionDelta, errorCorrectionType);
        var newRotation = GetCorrectedRotation(rotationDelta, errorCorrectionType);

        var newVelocity = Vector3.Lerp(Rigidbody.velocity, Rigidbody.velocity + velocityDelta, interpolationValue);
        var newAngularVelocity = Vector3.Lerp(Rigidbody.angularVelocity, Rigidbody.angularVelocity + angularVelocityDelta, interpolationValue);

        Rigidbody.MovePosition(newPosition);
        Rigidbody.MoveRotation(newRotation);

        if (errorCorrectionType == ErrorCorrectionType.SoftSync)
        {
            //Rigidbody.velocity = newVelocity;
            //Rigidbody.angularVelocity = newAngularVelocity;
        }

        if (errorCorrectionType == ErrorCorrectionType.Continious)
            return;

        serverState.Position = newPosition;
        serverState.Rotation = newRotation;
        serverState.Velocity = newVelocity;
        serverState.RotationVelocity = newAngularVelocity;
    }


    // TODO : refactor this!!!
    private Vector3 GetCorrectedPosition(Vector3 positionDelta, ErrorCorrectionType correctionType) 
    {
        switch (correctionType)
        {
            case ErrorCorrectionType.Limited:
                return Vector3.MoveTowards(Rigidbody.position, Rigidbody.position + positionDelta, NetworkSettings.SyncForce);
            default:
                return Vector3.Lerp(Rigidbody.position, Rigidbody.position + positionDelta, NetworkSettings.SyncForce);
        }
    }

    private Quaternion GetCorrectedRotation(Quaternion rotationDelta, ErrorCorrectionType correctionType)
    {
        switch (correctionType)
        {
            case ErrorCorrectionType.Limited:
                float maxDegrees = 180f;
                return Quaternion.RotateTowards(Rigidbody.rotation, rotationDelta * Rigidbody.rotation, NetworkSettings.SyncForce * maxDegrees);
            default:
                return Quaternion.Slerp(Rigidbody.rotation, rotationDelta * Rigidbody.rotation, NetworkSettings.SyncForce);
        }
    }


    private Vector3 GetDeltaPosition(RigidbodyState serverState, RigidbodyState clientState, ErrorCorrectionType correctionType)
    {
        switch (correctionType)
        {
            case ErrorCorrectionType.SoftSync:
                return serverState.Position - Rigidbody.position;
            case ErrorCorrectionType.Extrapolated:

                var ticksToExtrapolate = NetworkTime.CurrentTick - serverState.Tick;

                var extrapolatedServerPosition = serverState.Position + serverState.Velocity * Time.fixedDeltaTime * ticksToExtrapolate;

                serverStateTransform.position = extrapolatedServerPosition;

                return extrapolatedServerPosition - Rigidbody.position;
            default:
                return serverState.Position - clientState.Position;
        }
    }

    private Quaternion GetDeltaRotation(RigidbodyState serverState, RigidbodyState clientState, ErrorCorrectionType correctionType)
    {
        switch (correctionType)
        {
            case ErrorCorrectionType.SoftSync:
                return serverState.Rotation * Quaternion.Inverse(Rigidbody.rotation);
            case ErrorCorrectionType.Extrapolated:

                var ticksToExtrapolate = NetworkTime.CurrentTick - serverState.Tick;

                Vector3 angularDisplacement = serverState.RotationVelocity * Time.fixedDeltaTime * ticksToExtrapolate;

                float angleRad = angularDisplacement.magnitude;

                Quaternion extrapolatedServerRotation = serverState.Rotation;

                if (angleRad > 0.0001f)
                {
                    Vector3 axis = angularDisplacement.normalized;

                    Quaternion deltaRotation =
                        Quaternion.AngleAxis(angleRad * Mathf.Rad2Deg, axis);

                    extrapolatedServerRotation =
                        deltaRotation * serverState.Rotation;
                }

                serverStateTransform.rotation = extrapolatedServerRotation;

                return extrapolatedServerRotation * Quaternion.Inverse(Rigidbody.rotation);

            default:
                return serverState.Rotation * Quaternion.Inverse(clientState.Rotation);
        }
    }
}
