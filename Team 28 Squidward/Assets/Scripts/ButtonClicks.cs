using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TeamSquidward.Eric
{
    public class ButtonClicks : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        #region Variables

        [SerializeField]
        private Image _img;
        [SerializeField]
        private Sprite _default,_pressed;
        [SerializeField]
        private AudioClip _compressedClip, _umcompressedClip;
        [SerializeField]
        private AudioSource _source;

        #endregion

        #region Methods

        public void OnPointerDown(PointerEventData eventData)
        {
            _img.sprite = _pressed;
            _source.PlayOneShot(_compressedClip);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _img.sprite = _default;
            _source.PlayOneShot(_umcompressedClip);
        }

        public void OnClickEvent()
        {

        }

        #endregion

    }
}