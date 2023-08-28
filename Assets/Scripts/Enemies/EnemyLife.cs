using UnityEngine;

public class EnemyLife : MonoBehaviour, IDamagable
{
    [SerializeField] private float MaxLife;
    private float _life;
    private SpriteIndicatorBar _lifeBar;
    private void Start()
    {
        _life = MaxLife;
        _lifeBar = GetComponentInChildren<SpriteIndicatorBar>();
    }
    public void OnHit(ProjectileInfo info, Vector3 point, Vector3 normal)
    {
        _life -= info.Damage;
        _lifeBar.AnimateProgress(_life / MaxLife);
        if (_life <= 0)
        {
            Debug.Log("DED");
            _life = MaxLife;
            _lifeBar.SetProgress(1);
        }
    }
}
