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
                keyHandlers[key].Sort((a, b) => b.priority.CompareTo(a.priority)); // �켱���� ���� ������� ����
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

                // �ش� Key�� ����Ʈ�� ������� ���
                if (keyHandlers[key].Count == 0) {
                    keyHandlers.Remove(key);
                    frameCounters.Remove(key);
                }
                // �� �ڵ鷯�� ��Ȱ��ȭ �Ǹ鼭 �ֻ��� �켱������ ����Ǵ� ���
                else if (prevTopPriority == this) {
                    keyHandlers[key][0].OnBecameTopPriority?.Invoke();
                }
            }
        }

        void Update() {
            // OnKeyDown�� �� �����ӿ� �ѹ��� �ߵ� �� �� �ֵ��� frameCount�� üũ�Ѵ�.
            if (UnityEngine.Input.GetKeyDown(key) && IsTopPriority() && frameCounters[key] != Time.frameCount) {
                frameCounters[key] = Time.frameCount;
                OnKeyDown?.Invoke();
            }
        }

        public bool IsTopPriority() {
            if (keyHandlers.ContainsKey(key) && keyHandlers[key].Count > 0) {
                return keyHandlers[key][0] == this; // ���� �켱������ ���� ��ü���� Ȯ��
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