using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Ui.Contracts;
using Ca.Jwsm.Railroader.Api.Ui.Models;
using UnityEngine;

namespace Ca.Jwsm.Railroader.Api.Host.UI
{
    public sealed class OverlayTextPanelRenderer : MonoBehaviour
    {
        private const string RendererObjectName = "ca.jwsm.railroader.api.overlay-text-renderer";
        private const float PanelPadding = 8f;
        private const float PanelSpacing = 6f;
        private const float MaxPanelWidth = 320f;

        private static readonly List<PanelLayout> _topLeft = new List<PanelLayout>(8);
        private static readonly List<PanelLayout> _topRight = new List<PanelLayout>(8);
        private static readonly List<PanelLayout> _bottomLeft = new List<PanelLayout>(8);
        private static readonly List<PanelLayout> _bottomRight = new List<PanelLayout>(8);

        private IOverlayTextService _service;
        private GUIStyle _boxStyle;
        private GUIStyle _labelStyle;
        private Texture2D _backgroundTexture;

        private readonly struct PanelLayout
        {
            public PanelLayout(OverlayTextPanelDescriptor descriptor, OverlayTextPanelState state, float width, float height)
            {
                Descriptor = descriptor;
                State = state;
                Width = width;
                Height = height;
            }

            public OverlayTextPanelDescriptor Descriptor { get; }
            public OverlayTextPanelState State { get; }
            public float Width { get; }
            public float Height { get; }
        }

        internal static OverlayTextPanelRenderer EnsureCreated(IOverlayTextService service)
        {
            var existing = GameObject.Find(RendererObjectName);
            OverlayTextPanelRenderer renderer;
            if (existing != null)
            {
                renderer = existing.GetComponent<OverlayTextPanelRenderer>() ?? existing.AddComponent<OverlayTextPanelRenderer>();
            }
            else
            {
                var go = new GameObject(RendererObjectName);
                go.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(go);
                renderer = go.AddComponent<OverlayTextPanelRenderer>();
            }

            renderer.Initialize(service);
            return renderer;
        }

        internal static void DestroyExisting()
        {
            var existing = GameObject.Find(RendererObjectName);
            if (existing != null)
            {
                Destroy(existing);
            }
        }

        internal void Initialize(IOverlayTextService service)
        {
            _service = service;
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
        }

        private void OnGUI()
        {
            if (_service == null)
            {
                return;
            }

            EnsureStyles();
            CollectVisiblePanels();
            DrawTopAnchored(_topLeft, leftAligned: true, fromTop: true);
            DrawTopAnchored(_topRight, leftAligned: false, fromTop: true);
            DrawBottomAnchored(_bottomLeft, leftAligned: true);
            DrawBottomAnchored(_bottomRight, leftAligned: false);
        }

        private void OnDestroy()
        {
            if (_backgroundTexture != null)
            {
                Destroy(_backgroundTexture);
                _backgroundTexture = null;
            }
        }

        private void EnsureStyles()
        {
            if (_backgroundTexture == null)
            {
                _backgroundTexture = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false);
                _backgroundTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.55f));
                _backgroundTexture.Apply();
                _backgroundTexture.hideFlags = HideFlags.HideAndDontSave;
            }

            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box)
                {
                    border = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                    normal = { background = _backgroundTexture }
                };
            }

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.UpperLeft,
                    wordWrap = true,
                    fontSize = 14,
                    richText = false,
                    normal = { textColor = Color.white },
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0)
                };
            }
        }

        private void CollectVisiblePanels()
        {
            _topLeft.Clear();
            _topRight.Clear();
            _bottomLeft.Clear();
            _bottomRight.Clear();

            var descriptors = _service.GetDescriptors();
            if (descriptors == null || descriptors.Count == 0)
            {
                return;
            }

            for (int i = 0; i < descriptors.Count; i++)
            {
                var descriptor = descriptors[i];
                if (descriptor?.StateProvider == null)
                {
                    continue;
                }

                OverlayTextPanelState state;
                try
                {
                    state = descriptor.StateProvider();
                }
                catch
                {
                    continue;
                }

                if (state == null || !state.IsVisible || string.IsNullOrWhiteSpace(state.Text))
                {
                    continue;
                }

                float width = Mathf.Clamp(descriptor.MinWidth, 120f, MaxPanelWidth);
                float textHeight = Mathf.Max(18f, _labelStyle.CalcHeight(new GUIContent(state.Text), width - (PanelPadding * 2f)));
                float height = textHeight + (PanelPadding * 2f);
                var layout = new PanelLayout(descriptor, state, width, height);

                switch (descriptor.Anchor)
                {
                    case OverlayTextAnchor.TopRight:
                        _topRight.Add(layout);
                        break;
                    case OverlayTextAnchor.BottomLeft:
                        _bottomLeft.Add(layout);
                        break;
                    case OverlayTextAnchor.BottomRight:
                        _bottomRight.Add(layout);
                        break;
                    case OverlayTextAnchor.TopLeft:
                    default:
                        _topLeft.Add(layout);
                        break;
                }
            }
        }

        private void DrawTopAnchored(List<PanelLayout> panels, bool leftAligned, bool fromTop)
        {
            float cursor = 0f;
            for (int i = 0; i < panels.Count; i++)
            {
                var panel = panels[i];
                float x = leftAligned
                    ? panel.Descriptor.OffsetX
                    : Screen.width - panel.Descriptor.OffsetX - panel.Width;
                float y = panel.Descriptor.OffsetY + cursor;
                DrawPanel(new Rect(x, y, panel.Width, panel.Height), panel.State.Text);
                cursor += panel.Height + PanelSpacing;
            }
        }

        private void DrawBottomAnchored(List<PanelLayout> panels, bool leftAligned)
        {
            float cursor = 0f;
            for (int i = 0; i < panels.Count; i++)
            {
                var panel = panels[i];
                float x = leftAligned
                    ? panel.Descriptor.OffsetX
                    : Screen.width - panel.Descriptor.OffsetX - panel.Width;
                float y = Screen.height - panel.Descriptor.OffsetY - panel.Height - cursor;
                DrawPanel(new Rect(x, y, panel.Width, panel.Height), panel.State.Text);
                cursor += panel.Height + PanelSpacing;
            }
        }

        private void DrawPanel(Rect rect, string text)
        {
            GUI.Box(rect, GUIContent.none, _boxStyle);
            var labelRect = new Rect(
                rect.x + PanelPadding,
                rect.y + PanelPadding,
                rect.width - (PanelPadding * 2f),
                rect.height - (PanelPadding * 2f));
            GUI.Label(labelRect, text, _labelStyle);
        }
    }
}
