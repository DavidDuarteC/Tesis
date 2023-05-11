using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Cto")]
public class CtoItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isHM;

    public override string Name => base.Name + $": {move.Name}";

    public override bool Use(Approach approach)
    {
        //El movimiento se maneja desde la intefza de usuario del inventario, si se aprendio, devuelve verdadero
        return approach.HasMove(move);
    }

    public bool CanBeTaught(Approach approach)
    {
        return approach.Base.LearnableByItems.Contains(Move);
    }

    public override bool IsReusable => isHM;

    public override bool CanUseInBattle => false;

    public MoveBase Move => move;
}
