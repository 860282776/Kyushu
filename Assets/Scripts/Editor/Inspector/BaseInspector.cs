using UnityEditor;

namespace TGame.Editor.Inspector
{
	public class BaseInspector : UnityEditor.Editor
    {
        protected virtual bool DrawBaseGUI { get { return true; } }

        private bool isCompiling = false;
        protected virtual void OnInspectorUpdateInEditor() { }

        private void OnEnable()
        {
            OnInspectorEnable();
            EditorApplication.update += UpdateEditor;
        }
        protected virtual void OnInspectorEnable() { }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateEditor;
            OnInspectorDisable();
        }
        protected virtual void OnInspectorDisable() { }

        private void UpdateEditor()
        {
            //编辑器当前是否正在编译脚本？
            if (!isCompiling && EditorApplication.isCompiling)
            {
                isCompiling = true;
                OnCompileStart();
            }
            else if (isCompiling && !EditorApplication.isCompiling)
            {
                isCompiling = false;
                OnCompileComplete();
            }
            OnInspectorUpdateInEditor();
        }

        public override void OnInspectorGUI()
        {
            if (DrawBaseGUI)
            {
                base.OnInspectorGUI();
            }
        }

        protected virtual void OnCompileStart() { }
        protected virtual void OnCompileComplete() { }
    }
}