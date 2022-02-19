using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit;

public class FadeTeleportationProvider : TeleportationProvider
{

    [SerializeField] private RawImage FadeImage;
    [SerializeField] bool FadeTeleport = false;
    [Range(0,0.1f)]
    [SerializeField] float FadeInOutSpeed; // Speed to fade in, Speed to fade out.

    bool isFading = false;
    float timer = 0;

    private void Start()
    {
        FadeImage.color = Color.clear;
    }

    public override bool QueueTeleportRequest(TeleportRequest teleportRequest)
    {

        if (FadeTeleport)
        {
            StartCoroutine(FadeIn(teleportRequest));
        }
        else
        {
            currentRequest = teleportRequest;
            validRequest = true;
        }
        
        return true;
    }

    IEnumerator FadeIn(TeleportRequest teleportRequest)
    {
        timer = 0;
        FadeImage.color = Color.clear;

        while (timer < 1)
        {
            FadeImage.color = Color.Lerp(Color.clear, Color.black, timer);
            timer += FadeInOutSpeed;
            yield return new WaitForEndOfFrame();
        }

        currentRequest = teleportRequest;
        validRequest = true;

    }

    IEnumerator FadeOut()
    {
        timer = 0;
        FadeImage.color = Color.black;

        while (timer < 1)
        {
            FadeImage.color = Color.Lerp(Color.black, Color.clear, timer);
            timer += FadeInOutSpeed;
            yield return new WaitForEndOfFrame();
        }

        EndLocomotion();
    }

    protected override void Update()
    {
        if (!validRequest || !BeginLocomotion())
            return;


        var xrOrigin = system.xrOrigin;
        if (xrOrigin != null)
        {
            switch (currentRequest.matchOrientation)
            {
                case MatchOrientation.WorldSpaceUp:
                    xrOrigin.MatchOriginUp(Vector3.up);
                    break;
                case MatchOrientation.TargetUp:
                    xrOrigin.MatchOriginUp(currentRequest.destinationRotation * Vector3.up);
                    break;
                case MatchOrientation.TargetUpAndForward:
                    xrOrigin.MatchOriginUpCameraForward(currentRequest.destinationRotation * Vector3.up, currentRequest.destinationRotation * Vector3.forward);
                    break;
                case MatchOrientation.None:
                    // Change nothing. Maintain current origin rotation.
                    break;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(MatchOrientation)}={currentRequest.matchOrientation}.");
                    break;
            }

            var heightAdjustment = xrOrigin.Origin.transform.up * xrOrigin.CameraInOriginSpaceHeight;

            var cameraDestination = currentRequest.destinationPosition + heightAdjustment;

            xrOrigin.MoveCameraToWorldLocation(cameraDestination);
        }

        if (FadeTeleport)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            EndLocomotion();
        }
        

        validRequest = false;
    }
}

