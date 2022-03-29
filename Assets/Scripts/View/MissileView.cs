using System;
using ScriptableData;
using UnityEngine;
using Utils;

public class MissileView : MonoBehaviour
{
    public GameObject body;
    public ParticleSystem engineParticleSystem;
    
    private Vector3 _target;
    private float _timeBeforeStartEngine;
    private float _timeBeforeEngineStop;
    private float _initialImpulse;
    private float _engineAcceleration;
    private float _rotationSpeed;
    private Rigidbody _rb;
    private float _explosionArea;
    private float _explosionForce;
    private float _explosionDelay;
    private bool _isCollision = false;
    private const float ExplosionDestroy = 2f;
    private GameObject _explosionParticleSystem;

    public void SetParams(ScriptableData.ScriptableData data, Vector3 target)
    {
        _target = target;
        _timeBeforeStartEngine = data.Missile.timeBeforeStartEngine;
        _timeBeforeEngineStop = data.Missile.timeBeforeEngineStop;
        _initialImpulse = data.Missile.initialImpulse;
        _engineAcceleration = data.Missile.engineAcceleration;
        _rotationSpeed = data.Missile.rotationSpeed;
        _explosionArea = data.Missile.explosionArea;
        _explosionForce = data.Missile.explosionForce;
        _explosionDelay = data.Missile.explosionDelay;
        _explosionParticleSystem = data.Missile.explosionParticleSystem;
    }
    
    private void Start()
    {
        _rb = GetComponentInParent<Rigidbody>(); 
    }

    private void Update()
    {
        //_rb.velocity = transform.forward * _speed * Time.deltaTime;
        if (_isCollision)
        {
            if (_explosionDelay <= 0)
            {
                var hitsSphereCast = Physics.SphereCastAll(transform.position, _explosionArea, transform.forward,
                    _explosionArea, GlobalData.LayerForAim);
                foreach (var hitSphereCast in hitsSphereCast)
                {
                    if (hitSphereCast.rigidbody.isKinematic)
                    {
                        hitSphereCast.rigidbody.isKinematic = false;
                    }

                    hitSphereCast.rigidbody.AddForce(hitSphereCast.normal * _explosionForce, ForceMode.Impulse);
                }

                Destroy(gameObject);
            }
            else
            {
                _explosionDelay -= Time.deltaTime;
            }
        }
        else if (_timeBeforeStartEngine > 0)
        {
            _rb.velocity = _rb.transform.forward * _initialImpulse * Time.deltaTime;
            _timeBeforeStartEngine -= Time.deltaTime;
        }
        else if (_timeBeforeEngineStop > 0)
        {
            if (!engineParticleSystem.isPlaying)
            {
                engineParticleSystem.Play();
            }
            Vector3 direction = _target - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, _rotationSpeed * Time.deltaTime);
            
            _rb.velocity = _rb.transform.forward * _engineAcceleration * Time.deltaTime;
            _timeBeforeEngineStop -= Time.deltaTime;
        }
        else{
           //_rb.transform.forward = _rb.velocity.normalized;
           engineParticleSystem.Stop();
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        _isCollision = true;
        body.SetActive(false);
        var explosion = Instantiate(_explosionParticleSystem, transform.position, Quaternion.identity);
        Destroy(explosion, ExplosionDestroy);
    }
}