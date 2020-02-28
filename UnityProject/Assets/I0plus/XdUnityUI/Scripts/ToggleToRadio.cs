using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Baum2
{
    /**
     * このクラスの仕事はEditorできるはず
     */
    public class ToggleToRadio : MonoBehaviour
    {
        // 廃棄されないGameObjectこれにtoggleGroupをぶらさげ､マルチシーンでもつかえるグループにする
        private static GameObject _managerGameObject;

        private static readonly Dictionary<string, GameObject> ToggleGroupMap = new Dictionary<string, GameObject>();

        public static ToggleGroup GetToggleGroup(string name)
        {
            ToggleGroup toggleGroup;
            if (!ToggleGroupMap.ContainsKey(name))
            {
                // まだそのグループが存在しない場合は､GameObjectを作成
                var go = new GameObject(name);
                go.transform.SetParent(_managerGameObject.transform);
                // AddComponent･登録する
                toggleGroup = go.AddComponent<ToggleGroup>();
                ToggleGroupMap[name] = go;
            }
            else
            {
                // 存在する場合は利用する
                toggleGroup = ToggleGroupMap[name].GetComponent<ToggleGroup>();
            }

            return toggleGroup;
        }

        [SerializeField] private string groupName;

        public string GroupName
        {
            get => groupName;
        }

        public void SetGroupName(string name)
        {
            groupName = name;
            // 共有ToggleGroupを作成･取得する
            var toggleGroup = GetToggleGroup(groupName);

            // Toggleを取得し､グループを登録する
            var toggle = gameObject.GetComponent<Toggle>();
            toggle.group = toggleGroup;
        }

        private void Awake()
        {
            // まだその名前でToggleGroupがつくられていない
            if (_managerGameObject == null)
            {
                _managerGameObject = new GameObject("ToggleToRadioManager");
                // 廃棄されないオブジェクトにする
                DontDestroyOnLoad(_managerGameObject);
            }

            if (GroupName == null)
            {
                return;
            }

            // 初期化する
            SetGroupName(GroupName);
        }
    }
}