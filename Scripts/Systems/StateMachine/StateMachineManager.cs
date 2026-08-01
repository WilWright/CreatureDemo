#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Utils;

namespace StateMachine
{
    public class StateMachineManager : MonoBehaviour
    {
        [SerializeField] string[] _scriptsPath;
        [SerializeField] string _systemName;
        [SerializeField] string[] _states;
        [SerializeField] StateMachineManagerTemplates _templates;

        [MethodButton(nameof(CreateSystemFiles))]
        [SerializeField, Space(10)] MethodButton m_0;

        [MethodButton(nameof(CreateStates))]
        [SerializeField, Space(10)] MethodButton m_1;

        [MethodButton(nameof(CreateState), nameof(m_state))]
        [SerializeField, Space(10)] MethodButton m_2;
        [SerializeField] string m_state;

        enum Keyword
        {
            StateMachine,
            Controller,
            ParentState,
            State,
            StateType,
            StateTypeParameter,
            InitialState,
            ClassType
        }

        string[] _keywords;

        string[] _stateParameters;

        string _stateMachine;
        string _controller;
        string _parentState;
        string _stateType;
        string _stateTypeParameter;

        bool Init()
        {
            if (string.IsNullOrEmpty(_systemName))
            {
                SystemLog.Error("Missing System Name");
                return false;
            }

            if (_states.IsNullOrEmpty())
            {
                SystemLog.Error("Missing At Least 1 State");
                return false;
            }

            if (_templates == null)
            {
                SystemLog.Error("Missing Templates");
                return false;
            }

            bool invalid = false;
            for (int i = 0; i < _states.Length; i++)
            {
                if (string.IsNullOrEmpty(_states[i]))
                {
                    SystemLog.Error($"State {i} is invalid");
                    invalid = true;
                }
            }
            if (invalid)
            {
                return false;
            }

            _stateMachine       = $"{_systemName}StateMachine";
            _controller         = $"{_systemName}Controller";
            _parentState        = $"State_{_systemName}";
            _stateType          = $"{_systemName}State";
            _stateTypeParameter = $"{_systemName.ToFirstCharacterLower()}State";

            _stateParameters = new string[_states.Length];
            for (int i = 0; i < _states.Length; i++)
            {
                _states[i] = _states[i].Trim().Replace("\n", string.Empty);
                _stateParameters[i] = $"{_stateType}.{_states[i]}";
            }

            _keywords = new string[Enum.GetValues(typeof(Keyword)).Length];
            for (int i = 0; i < _keywords.Length; i++)
            {
                _keywords[i] = $"[[{(Keyword)i}]]";
            }

            return true;
        }

        void CreateSystemFiles()
        {
            if (Init() == false)
            {
                return;
            }

            CreateFile(_templates.StateMachine, _stateMachine, "Scripts", _systemName, "StateMachine");
            CreateFile(_templates.Controller  , _controller  , "Scripts", _systemName);
            CreateFile(_templates.ParentState , _parentState , "Scripts", _systemName, "StateMachine");
            CreateStatesFromSystem(true);

            ResolveAssets();
        }

        void CreateStates() => CreateStatesFromSystem(false);
        void CreateStatesFromSystem(bool createdSystems)
        {
            if (createdSystems == false && Init() == false)
            {
                return;
            }

            foreach (string state in _states)
            {
                CreateFile(_templates.State, $"{_stateType}_{state}", "Scripts", _systemName, "StateMachine", "States");
            }
        }

        void CreateState(string stateName)
        {
            if (_states.IsNullOrEmpty())
            {
                _states = new string[1];
                _states[0] = stateName;
            }
            else
            {
                if (_states.Contains(stateName) == false)
                {
                    List<string> newStates = new(_states)
                    {
                        stateName
                    };
                    _states = newStates.ToArray();
                }
            }

            Init();

            string scriptsPath = "Scripts";
            if (CollectionUtils.IsNullOrEmpty(_scriptsPath) == false)
            {
                var fullScriptsPath = new List<string>() { scriptsPath };
                fullScriptsPath.AddRange(_scriptsPath);
                scriptsPath = FileUtils.JoinAllPaths(fullScriptsPath.ToArray());
            }
            CreateFile(_templates.State, $"{_stateType}_{stateName}", scriptsPath, _systemName, "StateMachine", "States");
        }

        void CreateFile(TextAsset template, string fileName, params string[] filePath)
        {
            var path = Path.Join(FileUtils.GetEditorPath(filePath), fileName + ".cs");
            if (File.Exists(path))
            {
                SystemLog.Info($"{path} already exists");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            string[] templateLines = template.text.Split('\n');
            var file = File.Create(path);
            file.Close();

            using (var sw = new StreamWriter(path))
            {
                foreach (string line in templateLines)
                {
                    string l = line;
                    for (int i = 0; i < _keywords.Length; i++)
                    {
                        if (l == null)
                        {
                            break;
                        }

                        string k = _keywords[i];
                        if (l.Contains(k))
                        {
                            switch ((Keyword)i)
                            {
                                case Keyword.StateMachine      : ReplaceKeyword(_stateMachine      ); break;
                                case Keyword.Controller        : ReplaceKeyword(_controller        ); break;
                                case Keyword.ParentState       : ReplaceKeyword(_parentState       ); break;
                                case Keyword.StateType         : ReplaceKeyword(_stateType         ); break;
                                case Keyword.StateTypeParameter: ReplaceKeyword(_stateTypeParameter); break;
                                case Keyword.InitialState      : ReplaceKeyword(_stateParameters[0]); break;
                                case Keyword.ClassType         : ReplaceKeyword(fileName           ); break;

                                case Keyword.State:
                                    string lineCopy = l.Replace("\r\n", string.Empty)
                                                       .Replace("\r"  , string.Empty)
                                                       .Replace("\n"  , string.Empty);

                                    l = null;

                                    var lines = _states.Select(line => lineCopy.Replace(k, line)).ToList();
                                    sw.WriteLine(string.Join(",\n", lines));
                                    break;
                            }

                            void ReplaceKeyword(string replacement)
                            {
                                l = l.Replace(k, replacement);
                            }
                        }
                    }

                    if (l != null)
                    {
                        sw.Write(l);
                    }
                }
            }

            SystemLog.Info($"Created {path}");
        }

        void ResolveAssets()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
