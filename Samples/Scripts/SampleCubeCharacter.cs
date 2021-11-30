using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameMeanMachine.Unity.WindRose.NeighbourTeleports
{
    namespace Samples
    {
        [RequireComponent(typeof(MapObject))]
        public class SampleCubeCharacter : MonoBehaviour
        {
            private Camera camera;
            private MapObject obj;

            // Start is called before the first frame update
            void Start()
            {
                camera = Camera.main;
                obj = GetComponent<MapObject>();
            }

            // Update is called once per frame
            void Update()
            {
                camera.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    obj.Orientation = Types.Direction.DOWN;
                    obj.StartMovement(Types.Direction.DOWN);
                }
                else if (Input.GetKey(KeyCode.UpArrow))
                {
                    obj.Orientation = Types.Direction.UP;
                    obj.StartMovement(Types.Direction.UP);
                }
                else if (Input.GetKey(KeyCode.LeftArrow))
                {
                    obj.Orientation = Types.Direction.LEFT;
                    obj.StartMovement(Types.Direction.LEFT);
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    obj.Orientation = Types.Direction.RIGHT;
                    obj.StartMovement(Types.Direction.RIGHT);
                }
            }
        }
    }
}
