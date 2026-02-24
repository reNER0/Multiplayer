using System;
using System.Linq;
using Assets.Scripts.Commands;
using Assets.Scripts.Network;
using UnityEngine;

public class PhysicsObject : Predictable
{
    public Rigidbody Rigidbody;
    public Transform serverStateTransform;

    protected RigidbodyState[] RigidbodyStates => States as RigidbodyState[];


    protected override void Start()
    {
        base.Start();

        if (!NetworkRepository.Current.IsCurrentClientOwnerOfObject(this))
        {
            // TODO : Fix this. Commented because of object owner is setting too late
            //Destroy(serverStateTransform.gameObject);
            return;
        }

        serverStateTransform.parent = null;
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
            Rigidbody.angularVelocity,
            lastAppliedInputs
            );
    }


    public override void SaveCurrentState(int tick)
    {
        var state = new RigidbodyState(tick,
            Rigidbody.position,
            Rigidbody.velocity,
            Rigidbody.rotation,
            Rigidbody.angularVelocity,
            lastAppliedInputs
            );

        States[tick % 1024] = state;
    }

    protected virtual void FixedUpdate()
    {
        var serverState = lastServerState as RigidbodyState;

        if (serverState == null)
        {
            //Debug.LogError("Error while applying server predictable state!");
            return;
        }

        serverStateTransform.position = serverState.Position;
        serverStateTransform.rotation = serverState.Rotation;

        var localState = States.FirstOrDefault(x => x?.Tick == serverState.Tick);

        if (localState == null)
        {
            Debug.LogWarning($"Client received server state with tick {serverState.Tick}, " +
                $"but clients last state tick was {States.Where(x => x != null)?.OrderByDescending(x => x.Tick).First().Tick}");
            return;
        }

        var error = (serverState.Position - (localState as RigidbodyState).Position).magnitude;

        if (error >= NetworkSettings.MaximumError)
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
        var positionDelta = GetDeltaPosition(serverState, localState, errorCorrectionType);
        var rotationDelta = GetDeltaRotation(serverState, localState, errorCorrectionType);

        //var velocityDelta = serverState.Velocity - localState.Velocity;
        //var angularVelocityDelta = serverState.RotationVelocity - localState.RotationVelocity;

        var tickTimeInSeconds = Time.fixedDeltaTime;
        var pingTimeInSeconds = ClientHub.Ping / 1000f;
        var ticksToSmooth = Math.Max(NetworkTime.CurrentTick - serverState.Tick, 1);

        positionDelta /= ticksToSmooth;
        rotationDelta = Quaternion.Slerp(Quaternion.identity, rotationDelta, 1f / ticksToSmooth);

        var interpolationValue = (1f / ticksToSmooth) * NetworkSettings.SyncForce;

        var newPosition = GetCorrectedPosition(positionDelta, errorCorrectionType);
        var newRotation = GetCorrectedRotation(rotationDelta, errorCorrectionType);


        //var newVelocity = Vector3.Lerp(Rigidbody.velocity, Rigidbody.velocity + velocityDelta, interpolationValue);
        //var newAngularVelocity = Vector3.Lerp(Rigidbody.angularVelocity, Rigidbody.angularVelocity + angularVelocityDelta, interpolationValue);
        
        Rigidbody.MovePosition(newPosition);
        Rigidbody.MoveRotation(newRotation);

        // TODO : fix smooth sync for velocities. This is a temporary solution, but it causes some stuttering
        //Rigidbody.velocity = newVelocity;
        //Rigidbody.angularVelocity = newAngularVelocity;

        if (errorCorrectionType == ErrorCorrectionType.Continious)
            return;

        serverState.Position = newPosition;
        serverState.Rotation = newRotation;
        //serverState.Velocity = newVelocity;
        //serverState.RotationVelocity = newAngularVelocity;
    }

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
