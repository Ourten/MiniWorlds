using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameCode.Scripts.Inputs;
using UnityEngine;

namespace GameCode.Scripts.Gui
{
    public class GuiManager : MonoBehaviour
    {
        static GuiManager _Instance;

        public static GuiManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new GameObject("GuiManager").AddComponent<GuiManager>();
                return _Instance;
            }
        }

        private Dictionary<GuiReference, GameObject> _guiCanvasMap;
        private bool _isGuiOpen;

        private GuiManager()
        {
        }

        public void SwitchGuiState(GuiReference gui, bool state)
        {
            _guiCanvasMap[gui].SetActive(state);
            if (state)
                _guiCanvasMap[gui].GetComponent<IGui>()?.OnOpen();
            else
                _guiCanvasMap[gui].GetComponent<IGui>()?.OnClose();

            if (state)
            {
                _isGuiOpen = true;
                InputService.Singleton.IsCursorLocked = false;
                foreach (var canvas in _guiCanvasMap.Where(candidate => candidate.Key != gui)
                    .Select(pair => pair.Value))
                {
                    canvas.SetActive(false);
                    canvas.GetComponent<IGui>()?.OnClose();
                }
            }
            else if (!_guiCanvasMap.Any(pair => pair.Value.activeSelf))
            {
                _isGuiOpen = false;
                InputService.Singleton.IsCursorLocked = true;
            }
        }

        public void OpenGui(GuiReference gui)
        {
            SwitchGuiState(gui, true);
        }

        public void CloseGui(GuiReference gui)
        {
            SwitchGuiState(gui, false);
        }

        public List<GuiReference> GetOpenedGuis()
        {
            return _guiCanvasMap.Where(pair => pair.Value.activeSelf).Select(pair => pair.Key).ToList();
        }

        public List<GuiReference> GetClosedGuis()
        {
            return _guiCanvasMap.Where(pair => !pair.Value.activeSelf).Select(pair => pair.Key).ToList();
        }

        public bool IsAnyGuiOpen()
        {
            return _isGuiOpen;
        }

        public bool IsGuiOpen(GuiReference gui)
        {
            return _guiCanvasMap[gui].activeSelf;
        }

        private void Awake()
        {
            _guiCanvasMap = new Dictionary<GuiReference, GameObject>
            {
                {GuiReference.MAP, GameObject.Find("MinimapPanel")},
                {GuiReference.INGAME, GameObject.Find("IngamePanel")}
            };

            foreach (var gameObject in _guiCanvasMap.Values)
                gameObject.SetActive(false);
        }
    }
}