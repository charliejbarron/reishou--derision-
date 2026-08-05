using Godot;

[GlobalClass]
public partial class InputInterpreter : Resource
{
    public class Parameters
    {
        
    }
    
    public virtual void HandleNavigationInputs(PlayerInput.NavigationInput navigationInput, Player player)
    {
    }

    public virtual int HandleChunkNavigation(Split[] splits, Vector3 playerPosition)
    {
        return ChunkFunctions.CheckSplits(splits, playerPosition);
    }
}