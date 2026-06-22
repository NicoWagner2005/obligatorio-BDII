using TicketingMundialUCU.Data.Daos;

namespace TicketingMundialUCU.Services;

public class AuditoriaService(IAuditoriaDao dao)
{
    public Task<ResumenEstadisticas> GetResumenAsync() =>
        dao.GetResumenAsync();

    public Task<IEnumerable<EventoRanking>> GetRankingEventosAsync() =>
        dao.GetRankingEventosAsync();

    public Task<IEnumerable<CompradorRanking>> GetRankingCompradoresAsync() =>
        dao.GetRankingCompradoresAsync();

    public Task<IEnumerable<OcupacionSector>> GetOcupacionSectoresAsync() =>
        dao.GetOcupacionSectoresAsync();

    public Task<IEnumerable<MovimientoAuditoria>> GetHistorialCustodiaAsync(int limite = 100) =>
        dao.GetHistorialCustodiaAsync(limite);
}
