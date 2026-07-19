using Godot;
using Godot.Collections;


public partial class Player : RigidBody3D 
{
    private long _id = 0;
    private float speed = 7f;
    AudioStreamPlayer3D _audioStreamPlayer3D = null;
    Label3D _label = null;
    public bool IsLocalPlayer() {
        return _id == Multiplayer.GetUniqueId();
    }
    public void SetId(long id) {
        _id = id;
    }
    public override void _Ready()
    {
        base._Ready();
        _audioStreamPlayer3D = (AudioStreamPlayer3D)FindChild("AudioStreamPlayer3D");
        SetMultiplayerAuthority((int)_id);
        _label = GetNode<Label3D>("Label3D");
        _label.Text = _id.ToString();
        _label.Modulate = (IsLocalPlayer()) ? Colors.Green : Colors.Red;
    }
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);

        // We don't want the label rotating with the player
        Transform3D transform = Transform;
        _label.Transform = Transform;
        _label.GlobalTranslate(Vector3.Up);

        Array<Node3D> collidingBodies = GetCollidingBodies();
        if (collidingBodies == null) {
            return;
        }
        foreach (Node3D body in collidingBodies) {
            if (body is Player) {
                _audioStreamPlayer3D.Play();
            }
        }
        HandleInput();
    }
	void HandleInput() { // move player
        if (IsLocalPlayer()) {
            Vector2 input = Input.GetVector("left", "right", "forward", "backward");
            ApplyForce(new Vector3(input.X, 0, input.Y) * speed);
            GD.Print(input);
        }
	}
}