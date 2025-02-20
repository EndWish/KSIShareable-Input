using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KSIShareable.Input
{
    public class PriorityKeyHandler : MonoBehaviour
    {
        protected static Dictionary<KeyCode, List<PriorityKeyHandler>> keyHandlers
            = new Dictionary<KeyCode, List<PriorityKeyHandler>>();
        protected static Dictionary<KeyCode, int> frameCounters
            = new Dictionary<KeyCode, int>();

        private static void SortHandlers(KeyCode key) {
            if (keyHandlers.ContainsKey(key) && keyHandlers[key].Count >= 2) {
                var prevTopPriority = keyHandlers[key][0];
                keyHandlers[key].Sort((a, b) => b.priority.CompareTo(a.priority)); // 우선순위 높은 순서대로 정렬
                var curTopPriority = keyHandlers[key][0];

                if(curTopPriority != prevTopPriority) {
                    prevTopPriority.OnNoLongerTopPriority?.Invoke();
                    curTopPriority.OnBecameTopPriority?.Invoke();
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        [SerializeField] protected KeyCode key = KeyCode.Escape;
        [SerializeField] protected int priority = 0;
        public int Priority {
            get { return priority; }
            set {
                priority = value;
                if (keyHandlers.ContainsKey(key) && enabled) {
                    SortHandlers(key);
                }
            }
        }
        public bool TopPriorityOnEnable = false;

        public UnityEvent OnKeyDown;
        public UnityEvent OnBecameTopPriority;
        public UnityEvent OnNoLongerTopPriority;

        private void OnEnable() {
            if (!keyHandlers.ContainsKey(key)) {
                keyHandlers[key] = new List<PriorityKeyHandler>();
                frameCounters[key] = -1;
            }

            if (TopPriorityOnEnable && keyHandlers[key].Count > 0) {
                Priority = keyHandlers[key][0].Priority + 1;
            }

            keyHandlers[key].Add(this);
            if (IsTopPriority()) {
                OnBecameTopPriority?.Invoke();
            }
            else {
                OnNoLongerTopPriority?.Invoke();
            }
            SortHandlers(key);
        }

        private void OnDisable() {
            if (keyHandlers.ContainsKey(key)) {
                var prevTopPriority = keyHandlers[key][0];
                keyHandlers[key].Remove(this);
                if(prevTopPriority == this) {
                    OnNoLongerTopPriority?.Invoke();
                }

                // 해당 Key의 리스트가 비어지는 경우
                if (keyHandlers[key].Count == 0) {
                    keyHandlers.Remove(key);
                    frameCounters.Remove(key);
                }
                // 이 핸들러가 비활성화 되면서 최상위 우선순위가 변경되는 경우
                else if (prevTopPriority == this) {
                    keyHandlers[key][0].OnBecameTopPriority?.Invoke();
                }
            }
        }

        void Update() {
            // OnKeyDown이 한 프레임에 한번만 발동 될 수 있도록 frameCount를 체크한다.
            if (UnityEngine.Input.GetKeyDown(key) && IsTopPriority() && frameCounters[key] != Time.frameCount) {
                frameCounters[key] = Time.frameCount;
                OnKeyDown?.Invoke();
            }
        }

        public bool IsTopPriority() {
            if (keyHandlers.ContainsKey(key) && keyHandlers[key].Count > 0) {
                return keyHandlers[key][0] == this; // 가장 우선순위가 높은 객체인지 확인
            }
            return false;
        }
        public int GetTopPriority() {
            if (keyHandlers.ContainsKey(key) && keyHandlers[key].Count > 0) {
                return keyHandlers[key][0].Priority;
            }
            return -1;
        }
        public void SetAsTopPriority() {
            Priority = GetTopPriority() + 1;
        }

    }
}