using System.Reflection;
using UnityEngine;
//using UnityEditor;
#if hPHOTON2
using Photon.Pun;
#endif

namespace Passer {

    [System.Serializable]
    public class InputEvent {

        public enum InputType {
            None,
            Animator,
#if PLAYMAKER
            Playmaker,
#endif
            GameObject
        }

        //#region Input Action
        //public class InputAction {
        //    public string name;
        //    public virtual void SetComponent() { }
        //    public static InputAction Create(System.Type type) {
        //        if (type == typeof(bool))
        //            return new InputActionBool();
        //        return null;
        //    }
        //}
        //public class InputActionBool : InputEvent.InputAction {
        //    public bool value = false;
        //    public override void SetComponent() {
        //        value = EditorGUILayout.Toggle(value);
        //    }
        //}

        //public InputAction inputAction;
        //#endregion

        public int type;
        public GameObject targetGameObject; // the object on which the method is called
        public Object targetComponent; // the component on which the method is called
        public MethodInfo targetMethod; // the selected method

        private void SetMethodParameters(object methodTarget, MethodInfo method = null) {
            targetComponent = methodTarget as Object;
            if (targetComponent != null)
                targetGameObject = ((MonoBehaviour)targetComponent).gameObject;
            type = (int)InputType.GameObject;

            if (method == null)
                return;

            targetMethod = method;
            methodIndex = GetMethodIndex(method);
            methodName = targetMethod.Name;
            DetermineMethodProperties();
        }

        public delegate void VoidMethod();
        private VoidMethod voidMethod;
        public void SetMethod(VoidMethod method, EventType _eventType = EventType.Start) {
            eventType = _eventType;
            voidMethod = method;
            methodParameterCount = 0;
            SetMethodParameters(method.Target, method.Method);
        }

        public delegate void BoolMethod(bool b);
        private BoolMethod boolMethod;
        public void SetMethod(BoolMethod method, EventType _eventType = EventType.Change) {
            eventType = _eventType;
            boolMethod = method;
            methodParameterCount = 1;
            //MethodInfo methodInfo = method.Method; // method.Target.GetType().GetMethod(method.Method.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            SetMethodParameters(method.Target, method.Method);
        }

        public delegate void FloatMethod(float f);
        private FloatMethod floatMethod;
        public void SetMethod(FloatMethod method, EventType _eventType = EventType.Always) {
            eventType = _eventType;
            floatMethod = method;
            methodParameterCount = 1;
            SetMethodParameters(method.Target, method.Method);
        }

        public delegate void IntMethod(int x);
        private IntMethod intMethod;
        public void SetMethod(IntMethod method, EventType _eventType = EventType.Always) {
            eventType = _eventType;
            intMethod = method;
            methodParameterCount = 1;
            SetMethodParameters(method.Target, method.Method);
        }

        public void SetMethod(Object target, MethodInfo method) {
            ParameterInfo[] parameters = method.GetParameters();
            switch (parameters.Length) {
                case 0:
                    voidMethod = () => method.Invoke(target, new object[] { });
                    break;
                case 1:
                    if (parameters[0].ParameterType == typeof(bool))
                        boolMethod = (bool b) => method.Invoke(target, new object[] { b });
                    else if (parameters[0].ParameterType == typeof(float))
                        floatMethod = (float f) => method.Invoke(target, new object[] { f });
                    else if (parameters[0].ParameterType == typeof(int))
                        intMethod = (int x) => method.Invoke(target, new object[] { x });
                    break;
            }
        }

        public enum EventType {
            None,
            Start,
            End,
            Active,
            Inactive,
            Change,
            Always
        }
        public EventType eventType = EventType.None;

        #region Parameters
        public int methodParameterCount; // the number of parameters of the method

        public bool enumParameter; // is first parameter an enum?
        public int enumVal; // enum value for enum methods

        public bool valueChanged;
        public bool boolChanged;

        protected bool _boolValue;
        public virtual bool boolValue {
            get { return _boolValue; }
            set {
                valueChanged = (value != _boolValue);
                boolChanged = valueChanged;
                _boolValue = value;
                _floatValue = value ? 1 : 0;
                _intValue = value ? 1 : 0;
                Update();
            }
        }

        protected float _floatValue;
        public float floatTriggerLevel = 0.5F;
        public virtual float floatValue {
            get { return _floatValue; }
            set {
                valueChanged = (value != _floatValue);
                boolChanged = ((value > floatTriggerLevel) != _boolValue);
                _boolValue = (value > floatTriggerLevel);
                _floatValue = value;
                _intValue = (int)value;
                Update();
            }
        }

        protected int _intValue;
        public int intTriggerLevel = 0;
        public virtual int intValue {
            get { return _intValue; }
            set {
                valueChanged = (value != _intValue);
                boolChanged = ((value > intTriggerLevel) != _boolValue);
                _boolValue = (value > intTriggerLevel);
                _floatValue = value;
                _intValue = value;
            }
        }
        #endregion  

        public MethodInfo[] objMethods;
        public string[] objMethodNames;
        public Component[] objMethodComponents;
        public MethodInfo[] DetermineMethods() {
            if (targetGameObject == null) {
                objMethods = null;
                objMethodNames = null;
            }
            else {
                objMethods = GetSupportedMethods(targetGameObject, out objMethodNames, out objMethodComponents);
            }
            return objMethods;
        }

        private MethodInfo[] GetSupportedMethods(GameObject obj, out string[] methodNames, out Component[] methodComponents) {
            MethodInfo[] methods = new MethodInfo[0];
            methodNames = new string[0];
            methodComponents = new Component[0];

            if (obj == null)
                return methods;

            Component[] components = obj.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++) {
                if (components[i] == null || components[i] is Transform || components[i] is Animator || components[i] is ControllerInput)
                    continue;
#if hNW_PHOTON
                if (components[i] is PhotonView)
                    continue;
#endif

                string[] componentMethodNames;
                MethodInfo[] componentMethods = GetSupportedMethods(components[i].GetType(), out componentMethodNames);
#if UNITY_WSA_10_0 && !UNITY_EDITOR
                if (components[i].GetType() != typeof(HumanoidControl) && !components[i].GetType().GetTypeInfo().IsSubclassOf(typeof(Target)))
#else
                if (components[i].GetType() != typeof(HumanoidControl) && !components[i].GetType().IsSubclassOf(typeof(HumanoidTarget)))
#endif
                    AddComponentName(components[i].GetType().Name, ref componentMethodNames);

                methodNames = Extend(methodNames, componentMethodNames);
                methodComponents = Extend(methodComponents, components[i], componentMethodNames.Length - 1);
                methods = Extend(methods, componentMethods);
            }
            return methods;
        }

        public static MethodInfo[] GetSupportedMethods(System.Type type, out string[] methodNames) {
            MethodInfo[] targetMethods = typeof(HumanoidTarget).GetMethods(BindingFlags.Instance | BindingFlags.Public) ;
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);

            MethodInfo[] supportedMethods = new MethodInfo[methods.Length];
            int n = 0;
            for (int i = 0; i < methods.Length; i++) {
                if (!IsSupported(methods[i]))
                    continue;

                if (MethodInSet(methods[i], targetMethods))
                    continue;

                supportedMethods[n++] = methods[i];
            }

            supportedMethods = Truncate(supportedMethods, n);

            methodNames = new string[n + 1];
            methodNames[0] = " ";
            for (int i = 0; i < n; i++)
                methodNames[i + 1] = supportedMethods[i].Name;

            return supportedMethods;
        }

        private static bool IsSupported(MethodInfo method) {
            if (method.IsSpecialName)
                return false;
            if (!method.ReturnType.Equals(typeof(void)))
                return false;
            if (method.Name == "Awake" || method.Name == "Start" || method.Name == "Update")
                return false;

            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length > 2)
                return false;
            if (parameters.Length == 1 && !SupportedMethodParameter(parameters[0]))
                return false;
#if UNITY_WSA_10_0 && !UNITY_EDITOR
            if (parameters.Length == 2 && !parameters[0].ParameterType.GetTypeInfo().IsEnum && !SupportedMethodParameter(parameters[1]))
#else
            if (parameters.Length == 2 &&
                !(parameters[0].ParameterType.IsEnum) &&
                !SupportedMethodParameter(parameters[1]))
#endif
                return false;
            return true;
        }

        private static bool SupportedMethodParameter(ParameterInfo parameter) {
            return
                parameter.ParameterType.Equals(typeof(float)) ||
                parameter.ParameterType.Equals(typeof(int)) ||
                parameter.ParameterType.Equals(typeof(bool)) ||
                parameter.ParameterType.IsEnum;
        }

        public static void AddComponentName(string componentName, ref string[] methodNames) {
            string addText = componentName + ".";
            for (int i = 0; i < methodNames.Length; i++) {
                if (methodNames[i] != " ")
                    methodNames[i] = addText + methodNames[i];
            }
        }

        public static string[] Extend(string[] a1, string[] a2) {
            if (a1.Length > 0) {
                string[] r = new string[a1.Length + a2.Length - 1];
                // prevent copying elm 0 from a2 (this is the empty name)
                a2.CopyTo(r, a1.Length - 1);
                a1.CopyTo(r, 0);
                return r;
            }
            else {
                return a2;
            }
        }

        public static Component[] Extend(Component[] a1, Component a2, int n) {
            Component[] r = new Component[a1.Length + n];

            a1.CopyTo(r, 0);
            for (int i = a1.Length; i < a1.Length + n; i++)
                r[i] = a2;
            return r;
        }

        public static MethodInfo[] Extend(MethodInfo[] a1, MethodInfo[] a2) {
            MethodInfo[] r = new MethodInfo[a1.Length + a2.Length];
            a1.CopyTo(r, 0);
            a2.CopyTo(r, a1.Length);
            return r;
        }

        private static MethodInfo[] Truncate(MethodInfo[] methods, int n) {
            MethodInfo[] newMethods = new MethodInfo[n];
            for (int i = 0; i < n; i++)
                newMethods[i] = methods[i];
            return newMethods;
        }

        private static bool MethodInSet(MethodInfo method, MethodInfo[] methods) {
            for (int i = 0; i < methods.Length; i++)
                if (method.Name == methods[i].Name)
                    return true;
            return false;
        }

        [SerializeField]
        public int methodIndex; // index of the method in the methodlist, only necessary for editor
        [SerializeField]
        public string methodName;
        public System.Type methodParameterType; // type of the parameter

        protected MethodInfo GetMethod() {
            if (methodIndex <= 0)
                return null;

            DetermineMethods();
            if (objMethods == null)
                return null;

            if (methodIndex > objMethods.Length)
                return null;
            return objMethods[methodIndex - 1];
        }

        private int GetMethodIndex(MethodInfo method) {
            if (method == null)
                return 0;
            DetermineMethods();
            for (int i = 0; i < objMethods.Length; i++) {
                if (objMethods[i].Name == method.Name)
                    return i + 1;
            }
            return 0;
        }

        public void DetermineMethodProperties() {
            if (targetMethod == null)
                return;

            ParameterInfo[] parameters = targetMethod.GetParameters();
            methodParameterCount = parameters.Length;
            //input.obj = headTargetMethodComponent[input.methodIndex - 1];
#if UNITY_WSA_10_0 && !UNITY_EDITOR
            if (parameters.Length == 2 && parameters[0].ParameterType.GetTypeInfo().IsEnum) {
#else
            if ((parameters.Length == 1 || parameters.Length == 2) && parameters[0].ParameterType.IsEnum) {
#endif
                enumParameter = true;
                if (parameters.Length == 1)
                    methodParameterType = typeof(void);
                else
                    methodParameterType = parameters[1].ParameterType;
            }
            else if (parameters.Length == 1) {
                enumParameter = false;
                methodParameterType = parameters[0].ParameterType;
            }
            else {
                enumParameter = false;
                methodParameterType = typeof(void);
            }
        }

        public string[] GetEnumMethodValues() {
            ParameterInfo[] parameters = targetMethod.GetParameters();
            methodParameterCount = parameters.Length;
#if UNITY_WSA_10_0 && !UNITY_EDITOR
            if (parameters[0].ParameterType.GetTypeInfo().IsEnum) {
#else
            if (parameters[0].ParameterType.IsEnum) {
#endif
                return System.Enum.GetNames(parameters[0].ParameterType);
            }
            else
                return null;
        }

        protected void Update() {
            //if (targetComponent == null)
            //    return;

            switch (type) {
                case (int)InputType.Animator:
                    SetAnimatorParameter();
                    break;
#if PLAYMAKER
                case (int)InputType.Playmaker:
                    SendPlaymakerEvent();
                    break;
#endif
                case (int)InputType.GameObject:
                    if (targetMethod == null) {
                        targetMethod = GetMethod();
                        DetermineMethodProperties();
                    }
                    InvokeMethod(targetComponent);
                    break;
                default:
                    break;
            }
        }

        #region Method Invocation
        protected virtual void InvokeMethod(Object target) {
            //if (targetMethod == null)
            //    return;

            switch (methodParameterCount) {
                case 0:
                    InvokeVoidMethod(target);
                    break;
                case 1:
                    if (enumParameter)
                        InvokeEnumMethod(target);
                    else if (methodParameterType == typeof(GameObject))
                        InvokeGameObjectMethod(target);
                    else if (methodParameterType == typeof(Vector3))
                        InvokeVector3Method(target);
                    else if (methodParameterType == typeof(Quaternion))
                        InvokeQuaternionMethod(target);
                    else if (methodParameterType == typeof(float))
                        InvokeFloatMethod(target);
                    else if (methodParameterType == typeof(int))
                        InvokeIntMethod(target);
                    else if (methodParameterType == typeof(bool))
                        InvokeBoolMethod(target);
                    break;
                case 2:
                    if (enumParameter && enumVal > -1) {
                        if (methodParameterType == typeof(GameObject))
                            InvokeEnumGameObjectMethod(target);
                        else if (methodParameterType == typeof(Vector3))
                            InvokeEnumVector3Method(target);
                        else if (methodParameterType == typeof(Quaternion))
                            InvokeEnumQuaternionMethod(target);
                        else if (methodParameterType == typeof(float))
                            InvokeEnumFloatMethod(target);
                        else if (methodParameterType == typeof(int))
                            InvokeEnumIntMethod(target);
                        else if (methodParameterType == typeof(bool))
                            InvokeEnumBoolMethod(target);
                    }
                    break;
            }
        }

        protected void InvokeVoidMethod(Object target) {
            if (voidMethod != null) {
                InvokeMethod(() => voidMethod());
                return;
            }

            if (targetMethod == null)
                return;

            InvokeMethod(() => targetMethod.Invoke(target, new object[] { }));
        }

        protected void InvokeEnumMethod(Object target) {
            if (targetMethod == null)
                return;

            InvokeMethod(() => targetMethod.Invoke(target, new object[] { enumVal }));
        }

        protected virtual void InvokeGameObjectMethod(Object target) {
            return;
        }

        protected virtual void InvokeVector3Method(Object target) {
            return;
        }

        protected virtual void InvokeQuaternionMethod(Object target) {
            return;
        }

        protected void InvokeFloatMethod(Object target) {
            if (floatMethod != null) {
                InvokeMethod(() => floatMethod.Invoke(_floatValue));
                return;
            }

            if (targetMethod == null)
                return;

            InvokeMethod(() => targetMethod.Invoke(target, new object[] { _floatValue }));
        }

        protected void InvokeIntMethod(Object target) {
            if (intMethod != null) {
                intMethod(_intValue);
                return;
            }

            if (targetMethod == null)
                return;

            InvokeMethod(() => targetMethod.Invoke(target, new object[] { _intValue }));
        }

        protected void InvokeBoolMethod(Object target) {
            if (boolMethod != null) {
                InvokeMethod(() => boolMethod(_boolValue));
                return;
            }

            if (targetMethod == null)
                return;

            InvokeMethod(() => targetMethod.Invoke(target, new object[] { _boolValue }));
        }

        protected virtual void InvokeEnumGameObjectMethod(Object target) {
            return;
        }

        protected virtual void InvokeEnumVector3Method(Object target) {
            return;
        }

        protected virtual void InvokeEnumQuaternionMethod(Object target) {
            return;
        }

        protected void InvokeEnumFloatMethod(Object target) {
            if (targetMethod == null)
                return;

            InvokeMethod(() => targetMethod.Invoke(target, new object[] { enumVal, _floatValue }));
        }

        protected void InvokeEnumIntMethod(Object target) {
            if (targetMethod == null)
                return;

            InvokeMethod(() => targetMethod.Invoke(target, new object[] { enumVal, _intValue }));
        }

        protected void InvokeEnumBoolMethod(Object target) {
            if (targetMethod == null)
                return;

            InvokeMethod(() => targetMethod.Invoke(target, new object[] { enumVal, _boolValue }));
        }

        protected virtual void InvokeVector3QuaternionMethod(Object target) {
            return;
        }


        protected delegate void Func();
        protected void InvokeMethod(Func f) {
            switch (eventType) {
                case EventType.Active:
                    if (_boolValue)
                        f();
                    break;
                case EventType.Inactive:
                    if (!_boolValue)
                        f();
                    break;
                case EventType.Start:
                    if (_boolValue && boolChanged)
                        f();
                    break;
                case EventType.End:
                    if (!_boolValue && boolChanged)
                        f();
                    break;
                case EventType.Change:
                    if (valueChanged)
                        f();
                    break;
                case EventType.Always:
                    f();
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Animator
        public string animatorParameterName; // animator parameter name
        public AnimatorControllerParameterType animatorParameterType;
        public int animatorParameterIndex; // index of the animator parameter, only necessary for editor

        private void SetAnimatorParameter() {
            if (animatorParameterName == null)
                return;

            HumanoidControl humanoid = targetGameObject.GetComponent<HumanoidControl>();
            switch (animatorParameterType) {
                case AnimatorControllerParameterType.Bool:
                    humanoid.targetsRig.SetBool(animatorParameterName, _boolValue);
                    break;
                case AnimatorControllerParameterType.Float:
                    humanoid.targetsRig.SetFloat(animatorParameterName, _floatValue);
                    break;
                case AnimatorControllerParameterType.Int:
                    humanoid.targetsRig.SetFloat(animatorParameterName, _intValue);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    humanoid.targetsRig.SetTrigger(animatorParameterName);
                    break;

                default:
                    break;
            }
        }
        #endregion

        #region PlayMaker
#if PLAYMAKER
        public string playmakerEventName;
        public int playMakerEventIndex; // index of the playmaker event, only necessary for editor

        private void SendPlaymakerEvent() {
            PlayMakerFSM playmaker = targetGameObject.GetComponent<PlayMakerFSM>();
            if (playmaker != null) {
                switch (eventType) {
                    case EventType.Active:
                        if (_boolValue)
                            playmaker.SendEvent(playmakerEventName);
                        break;
                    case EventType.Inactive:
                        if (!_boolValue)
                            playmaker.SendEvent(playmakerEventName);
                        break;
                    case EventType.Start:
                        if (_boolValue && boolChanged)
                            playmaker.SendEvent(playmakerEventName);
                        break;
                    case EventType.End:
                        if (!_boolValue && boolChanged)
                            playmaker.SendEvent(playmakerEventName);
                        break;
                    case EventType.Change:
                        if (valueChanged)
                            playmaker.SendEvent(playmakerEventName);
                        break;
                    case EventType.Always:
                        playmaker.SendEvent(playmakerEventName);
                        break;
                    default:
                        break;
                }
            }
        }
#endif
        #endregion 
    }
}