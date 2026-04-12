using UnityEngine;
using System.Collections;

public class NPCController : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private Transform doorPosition;    // where NPC starts (door)
    [SerializeField] private Transform roomCenter;      // where NPC walks to

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;

    [Header("Sneeze")]
    [SerializeField] private ParticleSystem sneezeDust; // particle system on NPC
    [SerializeField] private int sneezeCount = 200;     // particles per sneeze
    [SerializeField] private float sneezeDelay = 2f;    // seconds after arriving before sneeze

    [Header("Timing")]
    [SerializeField] private float waitAfterSneeze = 3f; // seconds before walking back

    private bool _active = false;

    // Called by a UI button to start the NPC sequence
    public void StartSequence()
    {
        if (!_active)
            StartCoroutine(NPCSequence());
    }

    private IEnumerator NPCSequence()
    {
        _active = true;

        // 1. Walk to room center
        yield return StartCoroutine(WalkTo(roomCenter.position));

        // 2. Wait, then sneeze
        yield return new WaitForSeconds(sneezeDelay);
        Sneeze();

        // 3. Wait after sneeze
        yield return new WaitForSeconds(waitAfterSneeze);

        // 4. Walk back to door
        yield return StartCoroutine(WalkTo(doorPosition.position));

        _active = false;
    }

    private IEnumerator WalkTo(Vector3 target)
    {
        // Keep Y constant so NPC doesn't float or sink
        target.y = transform.position.y;

        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            Vector3 dir = (target - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;

            // Face direction of movement
            transform.LookAt(new Vector3(target.x, transform.position.y, target.z));

            yield return null;
        }
    }

    private void Sneeze()
    {
        if (sneezeDust != null)
            sneezeDust.Emit(sneezeCount);
    }
}
