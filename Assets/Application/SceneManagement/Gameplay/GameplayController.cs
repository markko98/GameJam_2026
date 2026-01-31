using UnityEngine;

public class GameplayController : USceneController
{
    private GameplayOutlet outlet;
    private DisposeBag disposeBag;

    public GameplayController() : base(SceneNames.Gameplay)
    {
    }
    
    public override void SceneDidLoad()
    {
        base.SceneDidLoad();
        disposeBag = new DisposeBag();
        ParticleProvider.Prewarm();
        outlet = GameObject.Find(OutletNames.Gameplay).GetComponent<GameplayOutlet>();
        var anim = new UILoadElementAnimation(outlet.dummyImage, 0.5f, Direction.Left);
        anim.Animate();
        PlayParticle();
    }

    private void PlayParticle()
    {
        var particle = ParticleProvider.GetParticle(ParticleType.Test);
        if (particle == null) return;
        var pc = particle.GetComponent<ParticleSystem>();
        
        particle.transform.position = outlet.particleTarget.position;
        particle.transform.localScale = Vector3.one;
        pc.Play();
        if (!Mathf.Approximately(2000, 0))
        {
            DelayedExecutionManager.ExecuteActionAfterDelay(2000,
                () =>
                {
                    particle.GetComponent<PoolableObject>().ReturnToPool();
                }).disposeBy(disposeBag);
        }
    }
}