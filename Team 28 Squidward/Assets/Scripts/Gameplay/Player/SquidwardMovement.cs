using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECM2.Common;
using ECM2.Characters;


namespace TeamSquidward.Eric
{
    public class SquidwardMovement : Character
    {
        #region Variables

        [SerializeField] private Animator farmerAnimation;
        [SerializeField] private LayerMask sheepLayer;

        [SerializeField] private float knockBackPower;

        [SerializeField] private GameObject visualGameObject;

        private bool IsPushingSheep;

        private GameObject objectPushing;

        #endregion

        #region Unity Methods

        private void OnCollisionEnter(Collision collision)
        {
            
            if (objectPushing == null && (sheepLayer.value & 1 << collision.gameObject.layer) == 128 )
            {
                objectPushing = collision.gameObject;
                IsPushingSheep = true;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (objectPushing == collision.gameObject)
            {
                IsPushingSheep = false;
                objectPushing = null;
            }
        }

        #endregion

        #region Methods

        protected override void Animate()
        {
            farmerAnimation.SetFloat("Speed", GetSpeed() );
            farmerAnimation.SetBool("IsPushingSheep", IsPushingSheep);

            if (GetVelocity().x > .1)
            {
                Vector3 temp = visualGameObject.transform.localScale;
                temp.x = -.5f;
                visualGameObject.transform.localScale = temp;

            }
            if (GetVelocity().x < -.1)
            {
                Vector3 temp = visualGameObject.transform.localScale;
                temp.x = .5f;
                visualGameObject.transform.localScale = temp;

            }
        }

        public void knockBack(Vector3 sheepLocation)
        {
            //print((sheepLocation - this.transform.position) * knockBackPower);
            
            Vector3 knockBackAmount = (this.transform.position - sheepLocation) * knockBackPower;
            knockBackAmount.z = 0;

            this.LaunchCharacter(knockBackAmount);
            //this.add
        }

        #endregion
    }
}