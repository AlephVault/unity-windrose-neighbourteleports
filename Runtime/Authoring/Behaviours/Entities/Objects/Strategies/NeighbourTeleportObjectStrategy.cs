using System;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects.Strategies;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects.Strategies.Base;
using GameMeanMachine.Unity.WindRose.NeighbourTeleports.Authoring.Behaviours.World.Layers.Objects.ObjectsManagementStrategies;
using UnityEngine;


namespace GameMeanMachine.Unity.WindRose.NeighbourTeleports
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities.Objects
            {
                namespace Strategies
                {
                    /// <summary>
                    ///   This strategy is just the counterpart of <see cref="NeighbourTeleportObjectsManagementStrategy"/>.
                    ///   This enables objects to be teleportable or not across maps in the same, or different, scope.
                    /// </summary>
                    public class NeighbourTeleportObjectStrategy : ObjectStrategy
                    {
                        /// <summary>
                        ///   The counterpart type is <see cref="NeighbourTeleportObjectsManagementStrategy"/>.
                        /// </summary>
                        protected override Type GetCounterpartType()
                        {
                            return typeof(NeighbourTeleportObjectsManagementStrategy);
                        }
                        
                        /// <summary>
                        ///   Whether the object is currently marked as teleportable
                        ///   or not (on false: the object will not be teleported at
                        ///   map boundaries as it would normally occur).
                        /// </summary>
                        [SerializeField]
                        private bool teleportable = true;

                        /// <summary>
                        ///   See <see cref="teleportable"/>.
                        /// </summary>
                        public bool Teleportable
                        {
                            get => teleportable;
                            set
                            {
                                bool oldTeleportable = teleportable;
                                teleportable = value;
                                PropertyWasUpdated("teleportable", oldTeleportable, teleportable);
                            }
                        }
                    }
                }
            }
        }
    }
}