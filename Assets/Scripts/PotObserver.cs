using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PotObserver : MonoBehaviour {
    public static PotObserver instance { get; private set; }
    //start with 4 or 5 stack variations
    [SerializeField]
    private GameObject _chipPrefab;
    //private List<GameObject> _stackInstances;
    private Vector3 _potBounds;
    private const int BASE_INCREMENT = 15;
    

    void Start() {
        instance = this;
        //_stackInstances = new List<GameObject>();
        _potBounds = GetComponent<MeshFilter>().mesh.bounds.extents;
    }

    public void IncrementFeedback(int incrementBy) {
        while (--incrementBy > 0) {
            var newCoin = Instantiate(
                _chipPrefab,
                transform.position + (Vector3.up * (incrementBy + 1)),
                Random.rotation,
                transform
            );
            StartCoroutine(_SleepCheck(newCoin.GetComponent<Rigidbody>()));
        }
    }

    public void ClearFeedback() {
        for (int i = 0; i < transform.childCount; ++i) {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private IEnumerator _SleepCheck(Rigidbody rigidbody) {
        do {
            if (rigidbody.transform.position.y < 0) {
                Destroy(rigidbody.gameObject);
                IncrementFeedback(1);
                break;
            }
            yield return new WaitForFixedUpdate();
        } while (!rigidbody.IsSleeping());
        Destroy(rigidbody);
    }
}
