using Godot;

[GlobalClass]
public partial class InputInterpreter : Resource
{
    public class Parameters
    {
        
    }
    
    public virtual void HandleMovementInputs(PlayerInput.NavigationInput navigationInput, Player player)
    {
    }

    public virtual int HandleSplits(Split[] splits, CharacterBody3D player)
    {
        
        
        return SplitsFuncs.CheckNavigationSplits(splits, player.GlobalPosition);
    }

    public virtual void HandleBlockers(Split[] blockers, Player player)
    {
        var clamped = SplitsFuncs.BlockPlayer(blockers, player);

        player.CharacterBody.GlobalPosition = clamped.newPosition;
        player.CharacterBody.Velocity = clamped.newVelocity;
    }
}