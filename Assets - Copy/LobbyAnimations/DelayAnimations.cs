using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayAnimations : MonoBehaviour
{
    //private Animator anim;

	// Use this for initialization
	void Start ()
    {
        StartCoroutine(SmallDelay());
        //anim.SetBool("bStartPlaying", true);
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    IEnumerator SmallDelay()
    {
        string tempstr = this.gameObject.name.Substring(this.gameObject.name.Length - 1);
        float waitthismuch = System.Convert.ToSingle(tempstr);
        
        yield return new WaitForSeconds(waitthismuch);
        this.GetComponent<Animator>().SetBool("bStartPlaying", true);
    }
}
