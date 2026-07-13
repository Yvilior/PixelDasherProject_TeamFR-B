using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GravityController : MonoBehaviour
{

    public enum Direction { Down, Up, Left, Right }

    [Header("Gravity")]
    [SerializeField] private float gravityStrength    = 30f;
    [SerializeField] private float transitionDuration = 0.2f;  

    [Header("Cooldown")]
    [SerializeField] private float flipCooldown = 0.3f;

    public event Action<Vector2> OnGravityChanged;


    private Rigidbody2D _rb;


    private Direction _current = Direction.Down;
    private Direction _target  = Direction.Down;

    private Quaternion _fromRot;
    private Quaternion _toRot;
    private float      _transitionT  = 1f; 
    private float      _cooldownTimer;

    public Vector2 Up => GetUp(_current);


    public Vector2 Down => -Up;


    public Vector2 Right => Vector2.right;

    public Direction CurrentDirection => _current;

    public bool IsTransitioning => _transitionT < 1f;

    public void Flip() => SetDirection(Opposite(_current));

    public void SetDirection(Direction newDir)
    {
        if (_cooldownTimer > 0f || newDir == _target) return;

        _cooldownTimer = flipCooldown;
        _target        = newDir;
        _fromRot       = transform.rotation;
        _toRot         = GetRotation(newDir);


        CancelVerticalVelocity();

        _transitionT = transitionDuration > 0f ? 0f : 1f;

        if (transitionDuration <= 0f)
            FinishTransition();
    }


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f; 
    }

    private void Update()
    {
        _cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.G))
            Flip();

        UpdateTransition();
    }

    private void FixedUpdate()
    {
        _rb.AddForce(Down * gravityStrength, ForceMode2D.Force);
    }



    private void UpdateTransition()
    {
        if (_transitionT >= 1f) return;

        _transitionT += Time.deltaTime / transitionDuration;

        if (_transitionT >= 1f)
        {
            _transitionT = 1f;
            FinishTransition();
            return;
        }

        float t = Mathf.SmoothStep(0f, 1f, _transitionT);
        transform.rotation = Quaternion.Slerp(_fromRot, _toRot, t);
    }

    private void FinishTransition()
    {
        _current           = _target;
        transform.rotation = _toRot;
        OnGravityChanged?.Invoke(Up);
    }

    private void CancelVerticalVelocity()
    {

        float verticalSpeed  = Vector2.Dot(_rb.linearVelocity, Up);
        _rb.linearVelocity  -= Up * verticalSpeed;
    }

    private static Vector2 GetUp(Direction dir) => dir switch
    {
        Direction.Down  =>  Vector2.up,
        Direction.Up    => -Vector2.up,
        Direction.Left  =>  Vector2.right,
        Direction.Right => -Vector2.right,
        _               =>  Vector2.up,
    };

    private static Quaternion GetRotation(Direction dir) => dir switch
    {
        Direction.Down  => Quaternion.identity,
        Direction.Up    => Quaternion.Euler(0f, 0f, 180f),
        Direction.Left  => Quaternion.Euler(0f, 0f, -90f),
        Direction.Right => Quaternion.Euler(0f, 0f,  90f),
        _               => Quaternion.identity,
    };

    private static Direction Opposite(Direction dir) => dir switch
    {
        Direction.Down  => Direction.Up,
        Direction.Up    => Direction.Down,
        Direction.Left  => Direction.Right,
        Direction.Right => Direction.Left,
        _               => Direction.Down,
    };
}