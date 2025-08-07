using System;
using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Target _target;
    [SerializeField] private GameObject _explosionPrefab;

    [Header("MOVEMENT")]
    [SerializeField] private float _speed = 15;
    [SerializeField] private float _acceleration = 5f;
    [SerializeField] private float _maxSpeed = 50f;

    [SerializeField] private float _rotateSpeed = 95;

    [Header("PREDICTION")]
    [SerializeField] private float _maxDistancePredict = 100;
    [SerializeField] private float _minDistancePredict = 5;
    [SerializeField] private float _maxTimePrediction = 5;
    private Vector3 _standardPrediction, _deviatedPrediction;

    [Header("DEVIATION")]
    [SerializeField] private float _deviationAmount = 50;
    [SerializeField] private float _deviationSpeed = 2;

    [Header("SELF-DESTRUCT")]
    [SerializeField] private float _maxLifetime = 10f;
    private float _lifetimeTimer = 0f;

    [Header("TARGET REACQUISITION")]
    [SerializeField] private float _reacquireRadius = 50f;
    [SerializeField] private float _maxReacquireAngle = 45f; // degrees
    [SerializeField] private LayerMask _targetLayer; // assign "Enemy" layer or similar

    void Start()
    {
        // ignore collisions with other missiles for the first second
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);
        foreach (var col in colliders)
        {
            if (col.gameObject != this.gameObject)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), col);
            }
        }
    }

    private void FixedUpdate()
    {

        if (_target == null || _target.Equals(null))
        {
            TryReacquireTarget();

            if (_target == null || _target.Equals(null))
            {
                _lifetimeTimer += Time.fixedDeltaTime;
                if (_lifetimeTimer >= _maxLifetime)
                {
                    SelfDestruct();
                    return;
                }
            }
        }
    
        _speed = Mathf.Min(_speed + _acceleration * Time.fixedDeltaTime, _maxSpeed);
        _rb.linearVelocity = transform.forward * _speed;

        var leadTimePercentage = Mathf.InverseLerp(_minDistancePredict, _maxDistancePredict, Vector3.Distance(transform.position, _target.transform.position));

        PredictMovement(leadTimePercentage);
        AddDeviation(leadTimePercentage);
        RotateRocket();
    }

    private void PredictMovement(float leadTimePercentage)
    {
        var predictionTime = Mathf.Lerp(0, _maxTimePrediction, leadTimePercentage);

        _standardPrediction = _target.Rb.position + _target.Rb.linearVelocity * predictionTime;
    }

    private void AddDeviation(float leadTimePercentage)
    {
        var deviation = transform.right * Mathf.Sin(Time.time * _deviationSpeed);

        var predictionOffset = transform.TransformDirection(deviation) * _deviationAmount * leadTimePercentage;

        _deviatedPrediction = _standardPrediction + predictionOffset;
    }

    private void RotateRocket()
    {
        var heading = _deviatedPrediction - transform.position;

        var rotation = Quaternion.LookRotation(heading);
        _rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, _rotateSpeed * Time.deltaTime));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_explosionPrefab)
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

        if (collision.transform.TryGetComponent<IExplode>(out var ex))
            ex.Explode();

        // detach and allow smoke to linger
        Transform smokeTrail = transform.Find("MissileFX/SmokeTrailFX");
        if (smokeTrail)
        {
            smokeTrail.SetParent(null, true); // keep world position
            smokeTrail.localScale = Vector3.one; // reset scale to avoid shrinkage

            var ps = smokeTrail.GetComponent<ParticleSystem>();
            if (ps)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(smokeTrail.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
            }
        }

        Destroy(gameObject); // destroy missile after detaching trail
    }

    private void SelfDestruct()
    {
        if (_explosionPrefab)
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

        // detach and stop smoke
        Transform smokeTrail = transform.Find("MissileFX/SmokeTrailFX");
        if (smokeTrail)
        {
            smokeTrail.SetParent(null, true);
            smokeTrail.localScale = Vector3.one;

            var ps = smokeTrail.GetComponent<ParticleSystem>();
            if (ps)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(smokeTrail.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
            }
        }

        Destroy(gameObject);
    }

    private void TryReacquireTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _reacquireRadius, _targetLayer);

        Transform bestTarget = null;
        float closestAngle = _maxReacquireAngle;

        foreach (var hit in hits)
        {
            Transform potential = hit.transform;
            Vector3 directionToTarget = (potential.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            if (angle <= closestAngle)
            {
                bestTarget = potential;
                closestAngle = angle;
            }
        }

        if (bestTarget != null)
        {
            _target = bestTarget.GetComponent<Target>();
        }
    }

        private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, _standardPrediction);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(_standardPrediction, _deviatedPrediction);
    }

    public void SetTarget(Target t)
    {
        _target = t;
    }
}