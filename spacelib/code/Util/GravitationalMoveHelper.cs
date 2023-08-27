using Sandbox;

namespace Sdt.Util;

public struct GravitationalMoveHelper
{
    
    public readonly Vector3 Down = Vector3.Down;
    public readonly Vector3 Up = Vector3.Up;
    
    private MoveHelper MoveHelper;

    public GravitationalMoveHelper(MoveHelper moveHelper, Vector3 down)
    {
        this.MoveHelper = moveHelper;
        this.Down = down;
        this.Up = -down;
    }

    public Vector3 Position {
        get => MoveHelper.Position;
        private set => MoveHelper.Position = value;
    }
    public Vector3 Velocity {
        get => MoveHelper.Velocity;
        private set => MoveHelper.Velocity = value;
    }

    public bool HitWall {
        get => MoveHelper.HitWall;
        set => MoveHelper.HitWall = value;
    }

    public float GroundBounce => MoveHelper.GroundBounce;
    public float WallBounce => MoveHelper.WallBounce;
    public float MaxStandableAngle  {
        get => MoveHelper.MaxStandableAngle;
        set => MoveHelper.MaxStandableAngle = value;
    }
    public Trace Trace  {
        get => MoveHelper.Trace;
        set => MoveHelper.Trace = value;
    }


    /// <summary>
    /// Trace this from one position to another
    /// </summary>
    public TraceResult TraceFromTo(Vector3 start, Vector3 end) => MoveHelper.TraceFromTo(start, end);

    /// <summary>
    /// Trace this from its current Position to a delta
    /// </summary>
    public TraceResult TraceDirection(Vector3 down) => MoveHelper.TraceDirection(down);
    
    /// <summary>
    /// Try to move to the position. Will return the fraction of the desired velocity that we traveled.
    /// Position and Velocity will be what we recommend using.
    /// </summary>
    public float TryMove( float timestep )
    {
        var timeLeft = timestep;
        float travelFraction = 0;
        HitWall = false;

        using var moveplanes = new VelocityClipPlanes( Velocity );

        for ( int bump = 0; bump < moveplanes.Max; bump++ )
        {
            if ( Velocity.Length.AlmostEqual( 0.0f ) )
                break;

            var pm = TraceFromTo( Position, Position + Velocity * timeLeft );

            travelFraction += pm.Fraction;

            if ( pm.Hit )
            {
                // There's a bug with sweeping where sometimes the end position is starting in solid, so we get stuck.
                // Push back by a small margin so this should never happen.
                Position = pm.EndPosition + pm.Normal * 0.03125f;
            }
            else
            {
                Position = pm.EndPosition;

                break;
            }

            moveplanes.StartBump( Velocity );

            if ( bump == 0 && pm.Hit && pm.Normal.Angle( Up ) >= MaxStandableAngle )
            {
                HitWall = true;
            }

            timeLeft -= timeLeft * pm.Fraction;

            if ( !moveplanes.TryAdd( pm.Normal, ref MoveHelper.Velocity, IsFloor( pm ) ? GroundBounce : WallBounce ) )
                break;
        }

        if ( travelFraction == 0 )
            Velocity = 0;

        return travelFraction;
    }
    
    /// <summary>
    /// Return true if this is the trace is a floor. Checks hit and normal angle.
    /// </summary>
    public bool IsFloor( TraceResult tr )
    {
        if ( !tr.Hit ) return false;
        return tr.Normal.Angle( Up ) < MaxStandableAngle;
    }

    /// <summary>
    /// Apply an amount of friction to the velocity
    /// </summary>
    public void ApplyFriction(float frictionAmount, float delta) => MoveHelper.ApplyFriction(frictionAmount, delta);

    /// <summary>
    /// Move our position by this delta using trace. If we hit something we'll stop,
    /// we won't slide across it nicely like TryMove does.
    /// </summary>
    public TraceResult TraceMove(Vector3 delta) => MoveHelper.TraceMove(delta);
    
    /// <summary>
    /// Like TryMove but will also try to step up if it hits a wall
    /// </summary>
    public float TryMoveWithStep( float timeDelta, float stepsize )
    {
        var startPosition = Position;

        // Make a copy of us to stepMove
        var stepMove = this;

        // Do a regular move
        var fraction = TryMove( timeDelta );

        // If it got all the way then that's cool, use it
        //if ( fraction.AlmostEqual( 0 ) )
        //	return fraction;

        // Move up (as much as we can)
        stepMove.TraceMove( Up * stepsize );

        // Move across (using existing velocity)
        var stepFraction = stepMove.TryMove( timeDelta );

        // Move back down
        var tr = stepMove.TraceMove( Down * stepsize );

        // if we didn't land on something, return
        if ( !tr.Hit ) return fraction;

        // If we landed on a wall then this is no good
        if ( tr.Normal.Angle( Up ) > MaxStandableAngle )
            return fraction;

        // if the original non stepped attempt moved further use that
        if ( startPosition.Distance( Position ) > startPosition.Distance( stepMove.Position ) )
            return fraction;

        // step move moved further, copy its data to us
        Position = stepMove.Position;
        Velocity = stepMove.Velocity;
        HitWall = stepMove.HitWall;

        return stepFraction;
    }
    
    /// <summary>
    /// Test whether we're stuck, and if we are then unstuck us
    /// </summary>
    public bool TryUnstuck()
    {
        var tr = TraceFromTo( Position, Position );
        if ( !tr.StartedSolid ) return true;

        return Unstuck();
    }
    
    /// <summary>
    /// We're inside something solid, lets try to get out of it.
    /// </summary>
    bool Unstuck()
    {

        //
        // Try going straight up first, people are most of the time stuck in the floor
        //
        for ( int i = 1; i < 20; i++ )
        {
            var tryPos = Position + Up * i;

            var tr = TraceFromTo( tryPos, Position );
            if ( !tr.StartedSolid )
            {
                Position = tryPos + tr.Direction.Normal * (tr.Distance - 0.5f);
                Velocity = 0;
                return true;
            }
        }

        //
        // Then fuck it, we got to get unstuck some how, try random shit
        //
        for ( int i = 1; i < 100; i++ )
        {
            var tryPos = Position + Vector3.Random * i;

            var tr = TraceFromTo( tryPos, Position );
            if ( !tr.StartedSolid )
            {
                Position = tryPos + tr.Direction.Normal * (tr.Distance - 0.5f);
                Velocity = 0;
                return true;
            }
        }

        return false;
    }
    
}