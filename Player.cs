using Godot;
using Godot.Collections;


public partial class Player : RigidBody3D
{
    private long _id = 0;
    private float speed = 7f;
    private const float DEADZONE = -1f;
    private bool _dead = false;
    AudioStreamPlayer3D _collisionAudioStreamPlayer = null;
    AudioStreamPlayer3D _deathAudioStreamPlayer = null;
    GpuParticles3D _gpuParticles3D = null;
    Label3D _label = null;
    public bool IsLocalPlayer()
    {
        return _id == Multiplayer.GetUniqueId();
    }
    public void SetId(long id)
    {
        _id = id;
    }
    public override void _Ready()
    {
        base._Ready();
        _collisionAudioStreamPlayer = (AudioStreamPlayer3D)FindChild("AudioStreamPlayer3D");
        _label = GetNode<Label3D>("Label3D");
        _label.Text = _id.ToString();
        _label.Modulate = (IsLocalPlayer()) ? Colors.Green : Colors.Red;
        _gpuParticles3D = GetNode<GpuParticles3D>("GPUParticles3D");
        _deathAudioStreamPlayer = (AudioStreamPlayer3D)FindChild("AudioStreamPlayer3D2");
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        // We don't want the label rotating with the player
        Transform3D transform = Transform;
        _label.Transform = Transform;
        _label.GlobalTranslate(Vector3.Up);

        Array<Node3D> collidingBodies = GetCollidingBodies();
        if (collidingBodies == null)
        {
            return;
        }
        foreach (Node3D body in collidingBodies)
        {
            if (body is Player)
            {
                _collisionAudioStreamPlayer.Play();
            }
        }
        HandleInput();
        HandleDeath();

    }
    void HandleInput()
    { // move player
        if (IsLocalPlayer())
        {
            Vector2 input = Input.GetVector("left", "right", "forward", "backward");
            Vector3 force = new Vector3(input.X, 0, input.Y) * speed;
            Rpc(MethodName.MovePlayer, force);
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    void MovePlayer(Vector3 force)
    {
        if (!Multiplayer.IsServer())
        {
            return;
        }
        ApplyForce(force);
    }
    void HandleDeath()
    {
        if (Transform.Origin.Y <= DEADZONE)
        {
            Die();
        }
    }
    void Die()
    {
        if (!_dead)
        {
            _gpuParticles3D.Emitting = true;
            _deathAudioStreamPlayer.Play();
        }
        _dead = true;
    }
}