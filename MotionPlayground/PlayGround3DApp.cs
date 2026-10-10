using System;
using Urho;
using Urho.Resources;

namespace MotionPlayground3D
{
    public enum AnimKind { None, Pulse, Rotate, Slide, Fade, Bounce }

    public class Playground3DApp : Application
    {
        Scene _scene;
        Node _modelNode;
        StaticModel _staticModel;
        Zone _zone;

        AnimKind _anim = AnimKind.None;
        bool _combineRotatePulse;
        float _speed = 1f;

        float _t;

        // если VM попросит загрузить модель до создания сцены — запомним
        string _pendingModelPath;
        string _pendingMaterialPath;
        Material _mat;                 // ЕДИНСТВЕННЫЙ материал на модели
        Technique _techDiff;
        Technique _techDiffAlpha;

        bool _resetPending;
        public Playground3DApp(ApplicationOptions opts) : base(opts) { }

        protected override void Start()
        {
            base.Start();
            CreateScene();

            // если что-то уже просили загрузить раньше — применим
            if (!string.IsNullOrWhiteSpace(_pendingModelPath))
                LoadModel(_pendingModelPath, _pendingMaterialPath);
            else
                LoadModel("Models/Cone.mdl", "Materials/Checker.xml");

        }

        void CreateScene()
        {
            _scene = new Scene();
            _scene.CreateComponent<Octree>();

            _zone = _scene.CreateComponent<Zone>();
            _zone.AmbientColor = new Color(1f, 1f, 0f);
            _zone.FogColor = new Color(0, 0, 0);

            var lightNode = _scene.CreateChild("light");
            lightNode.SetDirection(new Vector3(0.3f, -1f, 0.2f));
            var light = lightNode.CreateComponent<Light>();
            light.LightType = LightType.Directional;

            var cameraNode = _scene.CreateChild("camera");
            cameraNode.Position = new Vector3(0, 0.2f, 4.0f);
            cameraNode.LookAt(new Vector3(0, 0.2f, 0), Vector3.Up);

            var camera = cameraNode.CreateComponent<Camera>();
            Renderer.SetViewport(0, new Viewport(Context, _scene, camera, null));

            _modelNode = _scene.CreateChild("model");
            _modelNode.Position = new Vector3(0, 0, 0);
            _modelNode.Scale = new Vector3(1, 1, 1);

            _staticModel = _modelNode.CreateComponent<StaticModel>();
        }

        // ===== API для VM =====
        public void SetSpeed(double speedMultiplier) => _speed = (float)Math.Max(0.1, speedMultiplier);
        public void SetCombineRotatePulse(bool enabled) => _combineRotatePulse = enabled;

        public void SetAnimation(string key)
        {
            _t = 0f;
            _anim = AnimKind.None;

            switch (key)
            {
                case "pulse": _anim = AnimKind.Pulse; break;
                case "rotate": _anim = AnimKind.Rotate; break;
                case "slide": _anim = AnimKind.Slide; break;
                case "fade": _anim = AnimKind.Fade; break;
                case "bounce": _anim = AnimKind.Bounce; break;
            }

            _resetPending = true;
        }

        public void Stop()
        {
            _anim = AnimKind.None;
            _combineRotatePulse = false;
            _resetPending = true;
        }


        public void LoadModel(string modelPath, string materialPath = null)
        {
            if (_staticModel == null)
            {
                _pendingModelPath = modelPath;
                _pendingMaterialPath = materialPath;
                return;
            }

            var cache = ResourceCache;

            _staticModel.Model = cache.GetModel(modelPath);
           
            // техники
            _techDiff = cache.GetTechnique("Techniques/Diff.xml");
            _techDiffAlpha = cache.GetTechnique("Techniques/DiffAlpha.xml");

            // создаём один материал, который всегда живёт на StaticModel
            if (!string.IsNullOrWhiteSpace(materialPath))
            {
                var src = cache.GetMaterial(materialPath);
                _mat = src != null ? src.Clone() : new Material();
            }
            else
            {
                _mat = new Material();
            }

            if (_techDiff != null)
                _mat.SetTechnique(0, _techDiff, 1, 1);

            _mat.SetShaderParameter("MatDiffColor", new Color(1, 1, 1, 1));
            _staticModel.SetMaterial(_mat);

            _resetPending = true;
        }


        void ResetTransforms()
        {
            if (_modelNode == null) return;

            _modelNode.Position = new Vector3(0, 0, 0);
            _modelNode.Rotation = Quaternion.Identity;
            _modelNode.Scale = new Vector3(1, 1, 1);

            var mat = _staticModel != null ? _staticModel.GetMaterial(0) : null;
            if (mat != null)
                mat.SetShaderParameter("MatDiffColor", new Color(1, 1, 1, 1));
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
            if (_modelNode == null) return;

            if (_resetPending)
            {
                _resetPending = false;
                ResetTransforms();
            }

            var dt = timeStep * _speed;
            _t += dt;

            ApplyAnim(_anim, _t);

            if (_combineRotatePulse)
            {
                if (_anim != AnimKind.Rotate) ApplyAnim(AnimKind.Rotate, _t);
                if (_anim != AnimKind.Pulse) ApplyAnim(AnimKind.Pulse, _t);
            }
        }

        void ApplyAnim(AnimKind kind, float t)
        {
            switch (kind)
            {
                case AnimKind.Pulse:
                    {
                        var s = 1.0f + 0.15f * (float)Math.Sin(t * 2.0f * Math.PI);
                        _modelNode.Scale = new Vector3(s, s, s);
                        break;
                    }
                case AnimKind.Rotate:
                    {
                        var yaw = t * 120f;
                        _modelNode.Rotation = new Quaternion(0, yaw, 0);
                        break;
                    }
                case AnimKind.Slide:
                    {
                        var x = 0.7f * (float)Math.Sin(t * 2.0f * Math.PI);
                        var p = _modelNode.Position;
                        _modelNode.Position = new Vector3(x, p.Y, p.Z);
                        break;
                    }
                case AnimKind.Bounce:
                    {
                        var y = 0.5f * (float)Math.Abs(Math.Sin(t * 2.0 * Math.PI));
                        var p = _modelNode.Position;
                        _modelNode.Position = new Vector3(p.X, y, p.Z);
                        break;
                    }
                case AnimKind.Fade:
                    {
                        if (_mat == null) break;

                        if (_techDiffAlpha != null)
                            _mat.SetTechnique(0, _techDiffAlpha, 1, 1);

                        var a = 0.25f + 0.75f * (0.5f + 0.5f * (float)Math.Sin(t * 2.0f * Math.PI));
                        _mat.SetShaderParameter("MatDiffColor", new Color(1, 1, 1, a));
                        break;
                    }
                default:
                    {
                        // если не fade — возвращаем непрозрачную технику и альфу = 1
                        if (_mat != null && _techDiff != null)
                        {
                            _mat.SetTechnique(0, _techDiff, 1, 1);
                            _mat.SetShaderParameter("MatDiffColor", new Color(1, 1, 1, 1));
                        }
                        break;
                    }
            }
        }
    }
}
