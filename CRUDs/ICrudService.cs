namespace SettleDown.CRUDs;

public interface ICrudService<T> where T : class
{
    Task<T?> Create(T settleDownMember);
    Task<bool> Update(string id, T settleDownMember);
    Task Delete(string id);
    Task<T?> CreateWithoutSaving(T settleDownMember);
    Task<bool> UpdateWithoutSaving(string id, T settleDownMember);
    Task DeleteWithoutSaving(string id);
}