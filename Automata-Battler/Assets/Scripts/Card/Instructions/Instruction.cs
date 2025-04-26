using UnityEngine;
using System.Threading.Tasks;

public interface IInstruction
{
    public Task Execute(Card card);
    public void Rotate(int byAmount);
    public string GetVisual();
}
