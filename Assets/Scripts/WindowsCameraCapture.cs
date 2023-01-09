using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GeoSharpi.Capture;
using UnityEngine.UI;
#if UNITY_WSA
using UnityEngine.Windows.WebCam;


namespace JelleKUL.XRDataInteraction.Windows
{
    public class WindowsCameraCapture : BaseCameraCapture
    {
        private PhotoCapture photoCaptureObject = null;
        private Resolution cameraResolution = default(Resolution);
        private bool isCapturingPhoto, isReadyToCapturePhoto = false;
        private uint numPhotos = 0;
        private Text text;

        // Start is called before the first frame update
        void Start()
        {
            var resolutions = PhotoCapture.SupportedResolutions;
            if (resolutions == null || resolutions.Count() == 0)
            {
                if (text != null)
                {
                    text.text += "\n Resolutions not available. Did you provide web cam access?";
                }
                return;
            }
            cameraResolution = resolutions.OrderByDescending((res) => res.width * res.height).First();
            PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);

            if (text != null)
            {
                text.text += "\n Starting camera...";
            }
        }

        private void OnDestroy()
        {
            isReadyToCapturePhoto = false;

            if (photoCaptureObject != null)
            {
                photoCaptureObject.StopPhotoModeAsync(OnPhotoCaptureStopped);

                if (text != null)
                {
                    text.text += "\n Stopping camera...";
                }
            }
        }

        private void OnPhotoCaptureCreated(PhotoCapture captureObject)
        {
            if (text != null)
            {
                text.text += "\nPhotoCapture created...";
            }

            photoCaptureObject = captureObject;

            CameraParameters cameraParameters = new CameraParameters(WebCamMode.PhotoMode)
            {
                hologramOpacity = 0.0f,
                cameraResolutionWidth = cameraResolution.width,
                cameraResolutionHeight = cameraResolution.height,
                pixelFormat = CapturePixelFormat.BGRA32
            };

            captureObject.StartPhotoModeAsync(cameraParameters, OnPhotoModeStarted);
        }

        private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
        {
            if (result.success)
            {
                isReadyToCapturePhoto = true;

                if (text != null)
                {
                    text.text += "\n Ready! Press the above button to take a picture.";
                }
            }
            else
            {
                isReadyToCapturePhoto = false;

                if (text != null)
                {
                    text.text += "\n Unable to start photo mode!";
                }
            }
        }

        /// <summary>
        /// Takes a photo and attempts to load it into the scene using its location data.
        /// </summary>
        public override void TakeCameraImage()
        {
            if (!isReadyToCapturePhoto || isCapturingPhoto)
            {
                return;
            }

            isCapturingPhoto = true;

            if (text != null)
            {
                text.text += "\n Taking picture...";
            }

            photoCaptureObject.TakePhotoAsync(OnPhotoCaptured);
        }

        private void OnPhotoCaptured(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
        {
            if (result.success)
            {
                cameraTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
                photoCaptureFrame.UploadImageDataToTexture(cameraTexture);
                CreateNode();
                /*
                if (photoCaptureFrame.hasLocationData)
                {
                    Matrix4x4 cameraToWorldMatrix = Matrix4x4.identity;
                    photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);
                    CreateNode(pos = cameraToWorldMatrix);

                }
                */
                if (text != null)
                {
                    text.text += "\nTook picture!";
                }
                

            }
            else
            {
                if (text != null)
                {
                    text.text += "\nPicture taking failed: " + result.hResult;
                }
            }

            isCapturingPhoto = false;
        }

        private void OnPhotoCaptureStopped(PhotoCapture.PhotoCaptureResult result)
        {
            if (text != null)
            {
                text.text += "\n" + (result.success ? "Photo mode stopped." : "Unable to stop photo mode.");
            }

            photoCaptureObject.Dispose();
            photoCaptureObject = null;
        }


    }
}
#endif