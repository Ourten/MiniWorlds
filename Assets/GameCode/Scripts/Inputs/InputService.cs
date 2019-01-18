using System;
using System.Collections.Generic;
using GameCode.Scripts.Gui;
using UnityEngine;

namespace GameCode.Scripts.Inputs
{
    public class InputService
    {
        public static readonly InputService Singleton = new InputService();

        private readonly Dictionary<string, Action> _keyActions;

        private bool isCursorLocked;
        public bool IsCursorLocked
        {
            get => isCursorLocked;
            set
            {
                if (value)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }

                isCursorLocked = value;
            }
        }

        private InputService()
        {
            _keyActions = new Dictionary<string, Action>();
        }

        public void RegisterKey(string keyName, GuiReference toOpen)
        {
            RegisterKey(keyName, () =>
            {
                if (GuiManager.Instance.IsAnyGuiOpen() && GuiManager.Instance.IsGuiOpen(toOpen))
                    GuiManager.Instance.CloseGui(toOpen);
                else if (!GuiManager.Instance.IsAnyGuiOpen() && !GuiManager.Instance.IsGuiOpen(toOpen))
                    GuiManager.Instance.OpenGui(toOpen);
            });
        }

        public void RegisterKey(string keyName, Action toExecute)
        {
            _keyActions.Add(keyName, toExecute);
        }

        public bool HasKey(string keyName)
        {
            return _keyActions.ContainsKey(keyName);
        }

        public Action GetAction(string keyName)
        {
            return _keyActions[keyName];
        }

        public Dictionary<string, Action>.KeyCollection GetKeys()
        {
            return _keyActions.Keys;
        }
    }
}