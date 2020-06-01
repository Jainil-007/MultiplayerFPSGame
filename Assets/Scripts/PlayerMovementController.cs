using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerMovementController : MonoBehaviour
{
    public Joystick joystick;
    public FixedTouchField fixedTouchField;
   
    private RigidbodyFirstPersonController rigidBodyController;


    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        rigidBodyController = GetComponent<RigidbodyFirstPersonController>();
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void FixedUpdate()
    {
        rigidBodyController.joystickInputAxis.x = joystick.Horizontal;
        rigidBodyController.joystickInputAxis.y = joystick.Vertical;
        rigidBodyController.mouseLook.lookInputAxis = fixedTouchField.TouchDist;


        Debug.Log("Horizontal:" + joystick.Horizontal);
        Debug.Log("Vertical:" + joystick.Vertical);



        animator.SetFloat("Horizontal",joystick.Horizontal);
        animator.SetFloat("Vertical",joystick.Vertical);


        if (Mathf.Abs(joystick.Horizontal)>0.9f || Mathf.Abs(joystick.Vertical)>0.9f)
        {
            //Running

            rigidBodyController.movementSettings.ForwardSpeed = 16;
            animator.SetBool("IsRunning",true);


        }
        else
        {
            //Not Running
            rigidBodyController.movementSettings.ForwardSpeed = 8;
            animator.SetBool("IsRunning", false);



        }



    }

}
