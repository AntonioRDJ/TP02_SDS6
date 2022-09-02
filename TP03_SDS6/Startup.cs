using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace TP02_SDS6
{
    public class Startup
    {
        public Rootobject rootobject;
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

        }

        public void Configure(IApplicationBuilder app)
        {
            var builder = new RouteBuilder(app);
            builder.MapRoute("carro/criar", CreateXLS);
            builder.MapRoute("carro/ler", ReadXLS);
            builder.MapRoute("voosjson", ShowVoosJson);
            builder.MapRoute("/voosjson/gerar", GenerateVoosJson);


            var rotas = builder.Build();

            app.UseRouter(rotas);
        }

        public Task CreateXLS(HttpContext context)
        {
            string path = "D:\\Meus Arquivos\\Documentos\\Superior\\6º Semestre\\SDS\\TP03_SDS6\\TP03_SDS6\\carro.xls";
            XmlTextWriter xtw = new XmlTextWriter(path, Encoding.UTF8);
            xtw.Formatting = Formatting.Indented;

            xtw.WriteStartDocument();
            xtw.WriteStartElement("carro");
            xtw.WriteElementString("marca", "Ford");
            xtw.WriteElementString("modelo", "Fiesta");
            xtw.WriteElementString("cor", "preto");
            xtw.WriteEndElement();
            xtw.WriteEndDocument();
            xtw.Flush();
            xtw.Close();

            return context.Response.WriteAsync("foi");
        }

        public Task ReadXLS(HttpContext context)
        {
            var path = "D:\\Meus Arquivos\\Documentos\\Superior\\6º Semestre\\SDS\\TP03_SDS6\\TP03_SDS6\\carro.xls";
            XmlTextReader reader = new XmlTextReader(path);
            string content = "";
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        content += ("<" + reader.Name);
                        content += (">\n");
                        break;

                    case XmlNodeType.Text:
                        content += (reader.Value + "\n");
                        break;

                    case XmlNodeType.EndElement:
                        content += ("</" + reader.Name);
                        content += (">\n");
                        break;
                }
            }
            return context.Response.WriteAsync(content);
        }

        public Task ShowVoosJson(HttpContext context)
        {
            string fileName = "D:\\Meus Arquivos\\Documentos\\Superior\\6º Semestre\\SDS\\TP03_SDS6\\TP03_SDS6\\voos.json";
            string jsonString = File.ReadAllText(fileName);
            rootobject = JsonSerializer.Deserialize<Rootobject>(jsonString);

            var conteudoArquivo = CarregaArquivoHTML("formularioVoos");

            foreach (var voo in rootobject.voos)
            {
                conteudoArquivo = conteudoArquivo
                    .Replace("#NOVA-OPCAO#", $"<option value={voo.codigo}>{voo.codigo}</option>#NOVA-OPCAO#");
                conteudoArquivo = conteudoArquivo
                    .Replace("#NOVO-VOO#", $"<tr><td>{voo.codigo}</td><td>{voo.origem}</td><td>{voo.destino}</td><td>{voo.horario}</td><td>{voo.compania}</td><td>{voo.operando}</td></tr>#NOVO-VOO#");
            }
            conteudoArquivo = conteudoArquivo.Replace("#NOVA-OPCAO#", "");
            conteudoArquivo = conteudoArquivo.Replace("#NOVO-VOO#", "");

            return context.Response.WriteAsync(conteudoArquivo);
        }
        public Task GenerateVoosJson(HttpContext context)
        {
            var codigo = context.Request.Form["codigo"].First();
            Voo voo = new Voo();
            foreach (var v in rootobject.voos)
            {
                if (v.codigo == codigo)
                {
                    voo = v;
                    break;
                }
            }

            string fileName = @"D:\Meus Arquivos\Documentos\Superior\6º Semestre\SDS\TP03_SDS6\TP03_SDS6\vooSelecionado.json";
            string jsonString = JsonSerializer.Serialize(voo);
            File.WriteAllText(fileName, jsonString);

            return context.Response.WriteAsync(jsonString);
        }

        private string CarregaArquivoHTML(string nomeArquivo)
        {
            var nomeCompletoArquivo = $@"D:\Meus Arquivos\Documentos\Superior\6º Semestre\SDS\TP03_SDS6\TP03_SDS6\HTML\{nomeArquivo}.html";
            using (var arquivo = File.OpenText(nomeCompletoArquivo))
            {
                return arquivo.ReadToEnd();
            }
        }

    }
}
