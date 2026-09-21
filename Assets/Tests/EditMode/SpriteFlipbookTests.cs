using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class SpriteFlipbookTests
{
    private GameObject entityObject;
    private SpriteRenderer spriteRenderer;
    private Texture2D texture;
    private Sprite[] sprites;
    private SpriteAnimationData loopClip;
    private SpriteAnimationData nonLoopClip;

    [SetUp]
    public void SetUp()
    {
        entityObject = new GameObject("Entity");
        spriteRenderer = entityObject.AddComponent<SpriteRenderer>();

        texture = new Texture2D(2, 2);
        sprites = new Sprite[4];
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i] = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.zero);
        }

        loopClip = SpriteAnimationData.CreateForTest(sprites, 8f, true);
        nonLoopClip = SpriteAnimationData.CreateForTest(sprites, 8f, false);
    }

    [TearDown]
    public void TearDown()
    {
        if (entityObject != null)
        {
            Object.DestroyImmediate(entityObject);
        }

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
            {
                Object.DestroyImmediate(sprites[i]);
            }
        }

        if (texture != null)
        {
            Object.DestroyImmediate(texture);
        }

        if (loopClip != null)
        {
            Object.DestroyImmediate(loopClip);
        }

        if (nonLoopClip != null)
        {
            Object.DestroyImmediate(nonLoopClip);
        }
    }

    [Test]
    public void Play_AppliesFirstFrameSprite()
    {
        SpriteFlipbook flipbook = new SpriteFlipbook(spriteRenderer);

        flipbook.Play(loopClip);

        Assert.AreEqual(sprites[0], spriteRenderer.sprite);
        Assert.AreEqual(0, flipbook.Frame);
    }

    [Test]
    public void Tick_AdvancesFramesAtFrameRate()
    {
        SpriteFlipbook flipbook = new SpriteFlipbook(spriteRenderer);
        flipbook.Play(loopClip);

        flipbook.Tick(0.25f);

        Assert.AreEqual(2, flipbook.Frame);
        Assert.AreEqual(sprites[2], spriteRenderer.sprite);
    }

    [Test]
    public void Tick_LoopClip_WrapsToStart()
    {
        SpriteFlipbook flipbook = new SpriteFlipbook(spriteRenderer);
        flipbook.Play(loopClip);

        flipbook.Tick(0.5f);

        Assert.AreEqual(0, flipbook.Frame);
        Assert.AreEqual(sprites[0], spriteRenderer.sprite);
    }

    [Test]
    public void Tick_NonLoopClip_ClampsToLastFrame()
    {
        SpriteFlipbook flipbook = new SpriteFlipbook(spriteRenderer);
        flipbook.Play(nonLoopClip);

        flipbook.Tick(1f);

        Assert.AreEqual(sprites.Length - 1, flipbook.Frame);
        Assert.AreEqual(sprites[sprites.Length - 1], spriteRenderer.sprite);
    }

    [Test]
    public void Play_SameClipTwice_DoesNotResetFrame()
    {
        SpriteFlipbook flipbook = new SpriteFlipbook(spriteRenderer);
        flipbook.Play(loopClip);
        flipbook.Tick(0.25f);

        flipbook.Play(loopClip);

        Assert.AreEqual(2, flipbook.Frame);
    }

    [Test]
    public void Play_NullClip_TickDoesNotThrow()
    {
        SpriteFlipbook flipbook = new SpriteFlipbook(spriteRenderer);

        flipbook.Play(null);

        Assert.DoesNotThrow(() => flipbook.Tick(0.5f));
    }

    [Test]
    public void Stop_ThenPlaySameClip_RestartsFromFirstFrame()
    {
        SpriteFlipbook flipbook = new SpriteFlipbook(spriteRenderer);
        flipbook.Play(loopClip);
        flipbook.Tick(0.25f);

        flipbook.Stop();
        flipbook.Play(loopClip);

        Assert.AreEqual(0, flipbook.Frame);
        Assert.AreEqual(sprites[0], spriteRenderer.sprite);
    }
}
