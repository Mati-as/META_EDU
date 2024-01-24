
using System.Collections.Generic;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

    public class FallenLeaves_Image_Move : RaySynchronizer
    {
        private ParticleEventController _particleEventController;
        private GameObject uiCamera;

        
        public override void Init()
        {
            base.Init();
            GameObject.FindWithTag("GameManager").TryGetComponent(out _particleEventController);
        }

        public override void ShootRay()
        {
            Debug.Assert(_particleEventController != null);
           
            base.ShootRay();
            
         
            _particleEventController.ray = ray_ImageMove;

        }
    }
