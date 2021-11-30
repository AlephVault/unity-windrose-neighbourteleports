using System;
using System.Collections.Generic;
using System.Linq;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects.Strategies;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.World;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.World.Layers.Objects;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.World.Layers.Objects.ObjectsManagementStrategies;
using GameMeanMachine.Unity.WindRose.NeighbourTeleports.Authoring.Behaviours.Entities.Objects.Strategies;
using GameMeanMachine.Unity.WindRose.Types;
using UnityEngine;
using AlephVault.Unity.Support.Generic.Authoring.Types;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace GameMeanMachine.Unity.WindRose.NeighbourTeleports
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace World
            {
                namespace Layers
                {
                    namespace Objects
                    {
                        namespace ObjectsManagementStrategies
                        {
                            /// <summary>
                            ///   A neighbour teleport objects management strategy can connect to
                            ///   one or more maps (at most one map per direction) to teleport the
                            ///   objects that reach that map boundary. This strategy must NEVER
                            ///   be a main strategy.
                            /// </summary>
                            public class NeighbourTeleportObjectsManagementStrategy : ObjectsManagementStrategy
                            {
                                // Keeps a single link specifying: a target map, and the
                                // side of arrival in that target map.
                                [Serializable]
                                public class SideLink
                                {
                                    public Direction TargetSide;
                                    public NeighbourTeleportObjectsManagementStrategy Target;
                                }

                                /// <summary>
                                ///   Keeps the track of all the links a map has. Only one
                                ///   link per direction will exist.
                                /// </summary>
                                [Serializable]
                                public class SideLinks : AlephVault.Unity.Support.Generic.Authoring.Types.Dictionary<Direction, SideLink> {}

#if UNITY_EDITOR
                                [CustomPropertyDrawer(typeof(SideLinks))]
                                public class SideLinksPropertyDrawer : DictionaryPropertyDrawer {}
#endif

                                // Keeps the track of all the links this map has. Only one
                                // link per direction will exist.
                                [SerializeField]
                                private SideLinks links = new SideLinks();

                                // Gets the boundary size of the underlying map.
                                private static ushort BoundarySize(
                                    NeighbourTeleportObjectsManagementStrategy obj,
                                    Direction direction
                                )
                                {
                                    switch (direction)
                                    {
                                        case Direction.UP:
                                        case Direction.DOWN:
                                            return obj.StrategyHolder.Map.Width;
                                        default:
                                            return obj.StrategyHolder.Map.Height;
                                    }
                                }

                                // Checks whether the boundaries of two objects to be linked,
                                // with respect to the directions of interest, actually match
                                // or are different.
                                private static bool BoundarySizeMatches(
                                    NeighbourTeleportObjectsManagementStrategy first,
                                    Direction firstDirection,
                                    NeighbourTeleportObjectsManagementStrategy second,
                                    Direction secondDirection
                                )
                                {
                                    return BoundarySize(first, firstDirection) == BoundarySize(second, secondDirection);
                                }

                                /// <summary>
                                ///   Creates a link, bidirectional by default, to another map's
                                ///   neighbout-teleport strategy. It may occur that the target
                                ///   is actually the same object, to build cycles.
                                /// </summary>
                                /// <param name="fromSide">
                                ///   The side, in the current map, where the teleport is being
                                ///   initiated.
                                /// </param>
                                /// <param name="to">
                                ///   The map to teleport to. By using null, the existing link
                                ///   will be deleted, if any.
                                /// </param>
                                /// <param name="toSide">
                                ///   The side, in the new map, where the teleport process ends.
                                /// </param>
                                /// <param name="symmetric">
                                ///   Whether to execute a reciprocal call in the target.
                                /// </param>
                                public void Link(
                                    Direction fromSide, NeighbourTeleportObjectsManagementStrategy to,
                                    Direction toSide, bool symmetric = true
                                )
                                {
                                    if (!gameObject)
                                        throw new InvalidOperationException(
                                            "This object is destroyed - it cannot link or be linked to"
                                        );
                                    if (!to || !to.gameObject)
                                    {
                                        Unlink(fromSide, symmetric);
                                    }
                                    else
                                    {
                                        if (!BoundarySizeMatches(this, fromSide, to, toSide))
                                        {
                                            throw new ArgumentException(
                                                "The chosen strategies, in the chosen " +
                                                "directions, cannot be linked since they don't match " +
                                                "in boundary sizes"
                                            );
                                        }
                                        links[fromSide] = new SideLink {TargetSide = toSide, Target = to};
                                        if (symmetric) { to.Link(toSide, this, fromSide, false); }
                                    }
                                }

                                /// <summary>
                                ///   Destroys an existing link to a map's neighbour-teleport
                                ///   strategy, if any. 
                                /// </summary>
                                /// <param name="fromSide">
                                ///   The side, in the current map, where the teleport (if any)
                                ///   is being initiated and now must be removed.
                                /// </param>
                                /// <param name="symmetric">
                                ///   Whether to execute a reciprocal call in the target.
                                /// </param>
                                public void Unlink(Direction fromSide, bool symmetric = true)
                                {
                                    if (links.TryGetValue(fromSide, out SideLink other))
                                    {
                                        links.Remove(fromSide);
                                        if (symmetric) { other.Target.Unlink(other.TargetSide); }
                                    }
                                }

                                /// <summary>
                                ///   Unlinks the current neighbour-teleport strategy from all
                                ///   the directions.
                                /// </summary>
                                /// <param name="symmetric">
                                ///   Whether to execute a reciprocal call in each target.
                                /// </param>
                                public void Unlink(bool symmetric = true)
                                {
                                    Unlink(Direction.DOWN, symmetric);
                                    Unlink(Direction.LEFT, symmetric);
                                    Unlink(Direction.RIGHT, symmetric);
                                    Unlink(Direction.UP, symmetric);
                                }

                                /// <summary>
                                ///   Converts this map in a cycle on itself. A torus. The right
                                ///   boundary would teleport to the left side, and the left
                                ///   boundary would teleport to the right side. And the same
                                ///   for up/down directions.
                                /// </summary>
                                /// <param name="vertical">Whether to apply the cycle up/down</param>
                                /// <param name="horizontal">Whether to apply the cycle left/right</param>
                                public void Cycle(bool vertical = true, bool horizontal = true)
                                {
                                    if (vertical) Link(Direction.UP, this, Direction.DOWN);
                                    if (horizontal) Link(Direction.LEFT, this, Direction.RIGHT);
                                }

                                protected override void Awake()
                                {
                                    base.Awake();
                                    foreach (KeyValuePair<Direction, SideLink> pair in links.ToArray())
                                    {
                                        if (!Enum.IsDefined(typeof(Direction), pair.Key)
                                            || pair.Value == null
                                            || !Enum.IsDefined(typeof(Direction), pair.Value.TargetSide)
                                            || pair.Value.Target == null
                                            || !BoundarySizeMatches(
                                                this, pair.Key,
                                                pair.Value.Target, pair.Value.TargetSide))
                                        {
                                            Debug.LogWarning(
                                                $"Side link for direction {pair.Key} was " +
                                                $"removed on initialization. Fix the link in the " +
                                                $"editor and try again", this
                                            );
                                            links.Remove(pair.Key);
                                        }
                                    }
                                }

                                private void OnDestroy()
                                {
                                    Unlink();
                                }

                                protected override Type GetCounterpartType()
                                {
                                    return typeof(NeighbourTeleportObjectStrategy);
                                }

                                // Tells whether an object is not just positioned in the boundary but
                                // also it finished a movement in the direction of that boundary.
                                private bool IsHeadingOutward(MapObject mapObject, Direction? direction)
                                {
                                    if (mapObject.ParentMap != StrategyHolder.Map) return false;
                                    switch (direction)
                                    {
                                        case Direction.UP:
                                            return mapObject.Yf == StrategyHolder.Map.Height - 1;
                                        case Direction.DOWN:
                                            return mapObject.Y == 0;
                                        case Direction.LEFT:
                                            return mapObject.X == 0;
                                        case Direction.RIGHT:
                                            return mapObject.Xf == StrategyHolder.Map.Width - 1;
                                        default:
                                            return false;
                                    }
                                }
                                
                                // Tells whether the object has an appropriate shape to be teleported
                                // and to fit the target map.
                                private bool HasAppropriateShapeForThisMovement(
                                    MapObject obj, Direction? formerMovement
                                )
                                {
                                    // In this case, .Value will NOT cause an exception
                                    // since it passed the condition in the function
                                    // to check object heading.
                                    if (links.TryGetValue(formerMovement.Value, out SideLink link))
                                    {
                                        // 1. If the direction is between up/down and up/down or
                                        //    the direction is between left/right and left/right,
                                        //    the only requirement for the object is to fit the
                                        //    new map's width and height.
                                        // 2. Otherwise, it is required for the object to also
                                        //    have an NxN square shape.
                                        Map targetMap = link.Target.StrategyHolder.Map;
                                        if (obj.Width > targetMap.Width || obj.Height > targetMap.Height)
                                        {
                                            Debug.LogWarning(
                                                "The object will not be teleported: it does not fit inside " +
                                                "the target map"
                                            );
                                            return false;
                                        }
                                        if (!formerMovement.Value.SameAxis(link.TargetSide) && obj.Height != obj.Width)
                                        {
                                            Debug.LogWarning(
                                                "The object will not be teleported: the teleport would be " +
                                                "an axis-changing teleport, and objects must be square to not have " +
                                                "issues while teleporting"
                                            );
                                            return false;
                                        }

                                        return true;
                                    }

                                    return false;
                                }

                                // Gets the coordinates to attach this object into, in the target
                                // map after a teleport.
                                private static Tuple<ushort, ushort> GetAttachmentCoordinates(
                                    ushort objXi, ushort objYi, ushort objW, ushort objH,
                                    ushort targetW, ushort targetH, Direction from, Direction to
                                ) {
                                    if (from == to)
                                    {
                                        return new Tuple<ushort, ushort>(objXi, objYi);
                                    }

                                    switch (from)
                                    {
                                        case Direction.DOWN:
                                            switch (to)
                                            {
                                                case Direction.UP:
                                                    return new Tuple<ushort, ushort>(objXi, (ushort)(targetH - objH));
                                                case Direction.LEFT:
                                                    return new Tuple<ushort, ushort>(0, objXi);
                                                case Direction.RIGHT:
                                                    return new Tuple<ushort, ushort>(
                                                        (ushort)(targetW - objW),
                                                        (ushort)(targetH - objXi - objH)
                                                    );
                                                default:
                                                    return null;
                                            }
                                        case Direction.LEFT:
                                            switch (to)
                                            {
                                                case Direction.UP:
                                                    return new Tuple<ushort, ushort>(
                                                        (ushort)(targetW - objW - objYi),
                                                        (ushort)(targetH - objH)
                                                    );
                                                case Direction.RIGHT:
                                                    return new Tuple<ushort, ushort>((ushort)(targetW - objW), objYi);
                                                case Direction.DOWN:
                                                    return new Tuple<ushort, ushort>(objYi, 0);
                                                default:
                                                    return null;
                                            }
                                        case Direction.RIGHT:
                                            switch (to)
                                            {
                                                case Direction.UP:
                                                    return new Tuple<ushort, ushort>(
                                                        objYi,
                                                        (ushort)(targetH - objH)
                                                    );
                                                case Direction.LEFT:
                                                    return new Tuple<ushort, ushort>(0, objYi);
                                                case Direction.DOWN:
                                                    return new Tuple<ushort, ushort>((ushort)(targetW - objW - objYi), 0);
                                                default:
                                                    return null;
                                            }
                                        case Direction.UP:
                                            switch (to)
                                            {
                                                case Direction.LEFT:
                                                    return new Tuple<ushort, ushort>(
                                                        0,
                                                        (ushort)(targetH - objH - objXi)
                                                    );
                                                case Direction.RIGHT:
                                                    return new Tuple<ushort, ushort>(
                                                        (ushort)(targetW - objW),
                                                        objXi
                                                    );
                                                case Direction.DOWN:
                                                    return new Tuple<ushort, ushort>(objXi, 0);
                                                default:
                                                    return null;
                                            }
                                        default:
                                            return null;
                                    }
                                }

                                /// <summary>
                                ///   By the end of the movement, and after the events such as
                                ///   <see cref="MapObject.onMovementFinished" />, a maybe-teleport
                                ///   is processed for this object.
                                /// </summary>
                                /// <param name="strategy">The counterpart object strategy</param>
                                /// <param name="status">The in-strategy status (x, y, movement?)</param>
                                /// <param name="formerMovement">The last finished movement (same as status')</param>
                                /// <param name="stage">The stage this event is invoked. We will use "After" only</param>
                                public override void DoConfirmMovement(ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, Direction? formerMovement, string stage)
                                {
                                    NeighbourTeleportObjectStrategy ntStrategy = (NeighbourTeleportObjectStrategy) strategy;
                                    MapObject obj = strategy.StrategyHolder.Object;
                                    if (stage == "After" && ntStrategy != null && ntStrategy.Teleportable
                                        && IsHeadingOutward(obj, formerMovement)
                                        && HasAppropriateShapeForThisMovement(obj, formerMovement))
                                    {
                                        ushort x = obj.X;
                                        ushort y = obj.Y;
                                        ushort objW = obj.Width;
                                        ushort objH = obj.Height;
                                        obj.Detach();
                                        Direction direction = formerMovement.Value;
                                        SideLink link = links[direction];
                                        Map targetMap = link.Target.StrategyHolder.Map;
                                        Tuple<ushort, ushort> coordinates = GetAttachmentCoordinates(
                                            x, y, objW, objH, targetMap.Width, targetMap.Height,
                                            direction, link.TargetSide
                                        );
                                        Direction opposite = link.TargetSide.Opposite();
                                        DoTeleport(() =>
                                        {
                                            obj.Attach(targetMap, coordinates.Item1, coordinates.Item2, true);
                                            obj.CancelMovement();
                                            obj.Orientation = opposite;
                                            obj.StartMovement(opposite);
                                        }, obj, direction, link.Target, link.TargetSide);
                                    }
                                }

                                /// <summary>
                                ///   Performs the teleport action. Typically, this invokes the attach action
                                ///   directly, but may be customized. This method may be overriden but should
                                ///   not veto!
                                /// </summary>
                                /// <param name="teleport">The attach action</param>
                                /// <param name="objectBeingTeleported">The object being teleported</param>
                                /// <param name="fromSide">The boundary from which the object is parting</param>
                                /// <param name="to">The target strategy the object will arrive to</param>
                                /// <param name="toSide">The boundary into which the object is entering</param>
                                protected virtual void DoTeleport(Action teleport, MapObject objectBeingTeleported,
                                    Direction fromSide, NeighbourTeleportObjectsManagementStrategy to, Direction toSide)
                                {
                                    teleport();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
