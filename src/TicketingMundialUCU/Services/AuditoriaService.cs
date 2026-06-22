using TicketingMundialUCU.Data.Daos;

namespace TicketingMundialUCU.Services;

public class AuditoriaService(IAuditoriaDao dao)
{
    public Task<KpiAuditoria> GetKpiAsync() => dao.GetKpiAsync();

    public Task<IEnumerable<RankingEvento>> GetRankingEventosAsync() =>
        dao.GetRankingEventosAsync();

    public Task<IEnumerable<RankingComprador>> GetRankingCompradoresAsync() =>
        dao.GetRankingCompradoresAsync();

    public Task<IEnumerable<RankingFuncionario>> GetRankingFuncionariosAsync() =>
        dao.GetRankingFuncionariosAsync();

    public Task<EstadisticaTransferencias> GetEstadisticaTransferenciasAsync() =>
        dao.GetEstadisticaTransferenciasAsync();
}
