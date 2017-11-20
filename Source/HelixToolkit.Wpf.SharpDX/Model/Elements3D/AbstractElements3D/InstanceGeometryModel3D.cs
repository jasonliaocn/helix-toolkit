﻿using HelixToolkit.Wpf.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX.Utilities;
using SharpDX;
using SharpDX.Direct3D11;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace HelixToolkit.Wpf.SharpDX
{
    public abstract class InstanceGeometryModel3D : GeometryModel3D
    {
        /// <summary>
        /// List of instance matrix. 
        /// </summary>
        public IList<Matrix> Instances
        {
            get { return (IList<Matrix>)this.GetValue(InstancesProperty); }
            set { this.SetValue(InstancesProperty, value); }
        }

        /// <summary>
        /// List of instance matrix.
        /// </summary>
        public static readonly DependencyProperty InstancesProperty =
            DependencyProperty.Register("Instances", typeof(IList<Matrix>), typeof(InstanceGeometryModel3D), new AffectsRenderPropertyMetadata(null, InstancesChanged));

        /// <summary>
        /// 
        /// </summary>
        private static void InstancesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var model = (InstanceGeometryModel3D)d;
            model.InstanceBuffer.Instances = e.NewValue == null ? null : e.NewValue as IList<Matrix>;
            model.InstancesChanged();
            model.InvalidateRender();
        }

        public bool HasInstances { get { return InstanceBuffer.HasInstance; } }
        protected readonly InstanceBufferModel InstanceBuffer = new InstanceBufferModel();

        protected BoundingBox instancesBound;
        public BoundingBox InstancesBound
        {
            protected set
            {
                instancesBound = value;
            }
            get
            {
                return instancesBound;
            }
        }

        protected virtual void InstancesChanged()
        {
            UpdateInstancesBounds();           
        }

        protected virtual void UpdateInstancesBounds()
        {
            if (!HasInstances)
            {
                InstancesBound = this.BoundsWithTransform;
            }
            else
            {
                var bound = this.BoundsWithTransform.Transform(InstanceBuffer.Instances[0]);// BoundingBox.FromPoints(this.BoundsWithTransform.GetCorners().Select(x => Vector3.TransformCoordinate(x, instanceInternal[0])).ToArray());
                foreach (var instance in InstanceBuffer.Instances)
                {
                    var b = this.BoundsWithTransform.Transform(instance);// BoundingBox.FromPoints(this.BoundsWithTransform.GetCorners().Select(x => Vector3.TransformCoordinate(x, instance)).ToArray());
                    BoundingBox.Merge(ref bound, ref b, out bound);
                }
                InstancesBound = bound;
            }
        }

        protected override bool CanHitTest(IRenderMatrices context)
        {
            return base.CanHitTest(context) && geometryInternal != null && geometryInternal.Positions != null && geometryInternal.Positions.Count > 0;
        }
        /// <summary>
        /// 
        /// </summary>        
        public override bool HitTest(IRenderMatrices context, Ray rayWS, ref List<HitTestResult> hits)
        {
            if (CanHitTest(context))
            {
                if (this.InstanceBuffer.HasInstance)
                {
                    bool hit = false;
                    int idx = 0;
                    foreach (var modelMatrix in InstanceBuffer.Instances)
                    {
                        var b = this.Bounds;
                        this.PushMatrix(modelMatrix);
                        if (OnHitTest(context, rayWS, ref hits))
                        {
                            hit = true;
                            var lastHit = hits[hits.Count - 1];
                            lastHit.Tag = idx;
                            hits[hits.Count - 1] = lastHit;
                        }
                        this.PopMatrix();
                        ++idx;
                    }

                    return hit;
                }
                else
                {
                    return OnHitTest(context, rayWS, ref hits);
                }
            }
            else
            {
                return false;
            }
        }

        protected override bool CheckBoundingFrustum(ref BoundingFrustum boundingFrustum)
        {
            return !HasInstances && base.CheckBoundingFrustum(ref boundingFrustum) || boundingFrustum.Intersects(ref instancesBound);
        }

        protected override bool OnAttach(IRenderHost host)
        {
            if (base.OnAttach(host))
            {
                // --- init instances buffer            
                InstanceBuffer.Initialize(effect);
                InstancesChanged();
                (RenderCore as IGeometryRenderCore).InstanceBuffer = InstanceBuffer;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void OnDetach()
        {
            InstanceBuffer.DisposeAndClear();
            base.OnDetach();
        }
    }
}
