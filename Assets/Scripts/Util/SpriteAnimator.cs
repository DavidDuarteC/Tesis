using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator
{
    SpriteRenderer SpriteRenderer;
    List<Sprite> frames;
    float frameRate;

    int currentFrame;
    float timer;


    public SpriteAnimator(List<Sprite> frame, SpriteRenderer spriteRenderer, float framRate = 0.16f)
    {
        SpriteRenderer = spriteRenderer;
        this.frames = frame;
        this.frameRate = framRate;
    }

    public void Start()
    {
        currentFrame = 0; ;
        timer = 0f;
        SpriteRenderer.sprite = frames[0];
    }

    public void HandleUpdate()
    {
        timer += Time.deltaTime;
        if(timer > frameRate)
        {
            currentFrame = (currentFrame + 1) % frames.Count;
            SpriteRenderer.sprite = frames[currentFrame];
            timer -= frameRate;
        }
    }

    public List<Sprite> Frames
    {
        get { return frames; }
    }
}
