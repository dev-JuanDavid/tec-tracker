using Queue.Models;
using Queue.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue.DataBase
{
    public interface IRepositorio
    {
        List<ProgramsLicensedActivityViewModel> GetDataForInstalledVsLicensed(string idcompany, DateTime from, DateTime to, string user, string IdWorkArea);
        bool AddFileTranfer(List<FileTransferViewModel> model);
        List<FileTransferViewModel> ReportFileTransfer(string idCompany, string userName, DateTime startDate, DateTime endDate);
        List<WorkArea> ListArea(Guid companyId);
        List<Agent_Employee> ListUsuarioArea(Guid companyId, Guid idWorkArea);

        List<FunctionalityViewModel> ListFunctionality(string companyId);

       List<Agent_EmployeesGroups> ListGroups(Guid companyId);

    }
}
