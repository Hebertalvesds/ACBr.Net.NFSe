using ACBr.Net.Core.Extensions;
using ACBr.Net.Core.Logging;
using ACBr.Net.DFe.Core.Common;
using ACBr.Net.NFSe.Nota;
using ACBr.Net.NFSe.Providers;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Windows.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Starline.SmartNota.Util;
using ACBr.Net.NFSe.Demo.Configuracoes;
using ACbr.Net.Storage;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

namespace ACBr.Net.NFSe.Demo
{
    public partial class FormMain : Form, IACBrLog
    {
        #region Fields

        private ACBrNFSe acbrNFSe;
        private ACBrConfig config;
        private Preferencias formPreferencias;
        private string UriRetorno;
        private string UriEnvio;
        private string pathConfig;
        private string pathCidades;
        private string DBName;
        private LiteDB.LiteDatabase DB;
        private ACBrMunicipioNFSe municipio;
        #endregion Fields

        #region Constructors

        public FormMain()
        {
            InitializeComponent();
            formPreferencias = new Preferencias();
            pathConfig = Application.StartupPath + @"\Configuracoes";

            DB = Connect.OpenConnection(pathConfig);
            DBName = pathConfig + @"\SmartNotas.db";

            if (!Directory.Exists(pathConfig)) formPreferencias.Show();
            config = ACBrConfig.CreateOrLoad(Path.Combine(pathConfig, "nfse.config"));
            ComboBoxConfiguracoes();
        }

        #endregion Constructors

        #region Methods

        #region EventHandlers

        private void btnIniciarProc_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "";

            //Faz a leitura do arquivo de Envio ou Consulta de Lotes e Decide qual processo irá chamar
            if (UriEnvio.IsEmpty())
                MessageBox.Show(this, "Caminho do XML de Envio não pode ser vázio!", "Aviso!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (UriRetorno.IsEmpty())
                MessageBox.Show(this, "Caminho do XML de Retorno não pode ser vázio!", "Aviso!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                acbrNFSe.Certificado = SelectCertificate(GetCertificatesStore());
                acbrNFSe.Configuracoes.WebServices.Ambiente = cmbAmbiente.GetSelectedValue<DFeTipoAmbiente>();
                municipio = (ACBrMunicipioNFSe)cmbCidades.SelectedItem;

                Processamento();
            }

        }

        private void Processamento()
        {
            this.Log().Info("Lendo Arquivos de: " + UriEnvio);
            var url = new UriBuilder(UriEnvio);
            string html = TWeb.FetchRps(url.Uri);
            btnIniciarProc.Enabled = false;

            do
            {
                
                if (html.IsEmpty())
                {
                    this.Log().Info($"Não foram encontrados arquivos de Envio ou Consulta no caminho {UriEnvio}");
                    btnIniciarProc.Enabled = true;
                    break;
                }

                var xml = XDocument.Parse(html);

                var root = xml.Root;
                if (root.Name.ToString().Contains("EnviarLoteRps"))
                {
                    var retorno = EnviarLoteRpsEnvio(xml);
                    string numeroLote = "";
                    if (retorno.Sucesso)
                    {
                        this.Log().Info("Sucesso no processamento! Gravando no repositório de processamento: " + UriRetorno);
                        numeroLote = retorno.NumeroLote;
                        var xmlRetonro = RemoveAllNamespaces(XElement.Parse(retorno.XmlRetorno)).ToString();
                        ResultadoEnvioLoteRps(numeroLote, xmlRetonro, UriRetorno);
                    }
                    else
                    {
                        this.Log().Error($"Erro no envio do Lote {numeroLote}");
                        var xmlRetonro = RemoveAllNamespaces(XElement.Parse(retorno.XmlRetorno)).ToString();
                        ResultadoEnvioLoteRps(numeroLote, xmlRetonro, UriRetorno);
                    }
                }
                else if (root.Name.ToString().Contains("ConsultarLoteRps"))
                {
                    var protocoloConsulta = BuscaNumeroProtocolo(xml);
                    var retorno = ConsultarLoteRps(xml, protocoloConsulta);
                    if (retorno.Sucesso)
                    {
                        this.Log().Info("Sucesso na Consulta! Gravando no repositório de processamento: " + UriRetorno);
                        var xmlRetonro = RemoveAllNamespaces(XElement.Parse(retorno.XmlRetorno)).ToString();
                        ResultadoConsultaLoteRps(protocoloConsulta, xmlRetonro, UriRetorno);
                    }
                    else
                    {
                        this.Log().Error($"Arquivo de consulta para protocolo {protocoloConsulta} retornou erros.");
                        var xmlRetonro = RemoveAllNamespaces(XElement.Parse(retorno.XmlRetorno)).ToString();
                        ResultadoConsultaLoteRps(protocoloConsulta, xmlRetonro, UriRetorno);
                    }
                }
                else
                {
                    MessageBox.Show("A operação requisita não pode ser processada. Ausência de Elemento EnviarLoteRps ou ConsultarLoteRps");
                    throw new ApplicationException("Processamento Interrompido");
                }

            } while (!(html = TWeb.FetchRps(url.Uri)).IsEmpty());
        }

        private void btnConsultarSituacao_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                var numero = 10;
                if (InputBox.Show("Numero Lote", "Digite o numero do lote.", ref numero).Equals(DialogResult.Cancel)) return;

                var protocolo = "0";
                if (InputBox.Show("Numero do Protocolo", "Digite o numero do protocolo.", ref protocolo).Equals(DialogResult.Cancel)) return;

                var ret = acbrNFSe.ConsultarSituacao(numero, protocolo);
                wbbDados.LoadXml(ret.XmlEnvio);
                wbbResposta.LoadXml(ret.XmlRetorno);
            });
        }

        private void btnConsultarNFSeRps_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                var numero = "10";
                if (InputBox.Show("Numero da RPS", "Digite o numero da RPS.", ref numero).Equals(DialogResult.Cancel)) return;

                var serie = "0";
                if (InputBox.Show("Serie da RPS", "Digite o numero da serie da RPS.", ref serie).Equals(DialogResult.Cancel)) return;

                var ret = acbrNFSe.ConsultaNFSeRps(numero, serie, TipoRps.RPS);
                wbbDados.LoadXml(ret.XmlEnvio);
                wbbResposta.LoadXml(ret.XmlRetorno);
            });
        }

        private void btnConsultarNFSePeriodo_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                var ret = acbrNFSe.ConsultaNFSe(DateTime.Today.AddDays(-7), DateTime.Today);
                wbbDados.LoadXml(ret.XmlEnvio);
                wbbResposta.LoadXml(ret.XmlRetorno);
            });
        }

        private void btnCancelarNFSe_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                var codigo = "0001";
                if (InputBox.Show("Código de cancelamento", "Código de cancelamento.", ref codigo).Equals(DialogResult.Cancel)) return;

                var serie = "0";
                if (InputBox.Show("Numero NFSe", "Digite o numero da NFSe.", ref serie).Equals(DialogResult.Cancel)) return;

                var motivo = "";
                if (InputBox.Show("Motivo Cancelamento", "Digite o motivo do cancelamento.", ref motivo).Equals(DialogResult.Cancel)) return;

                var ret = acbrNFSe.CancelaNFSe(codigo, serie, motivo);
                wbbDados.LoadXml(ret.XmlEnvio);
                wbbResposta.LoadXml(ret.XmlRetorno);
            });
        }

        private void btnSelecionarArquivo_Click(object sender, EventArgs e)
        {
            LoadMunicipios();
        }

        private void btnAdicionar_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                var municipio = new ACBrMunicipioNFSe();
                if (FormEdtMunicipio.Editar(municipio).Equals(DialogResult.Cancel)) return;

                AddMunicipio(municipio);
            });
        }

        private void btnCopiar_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                if (lstMunicipios.SelectedItems.Count < 1) return;

                if (MessageBox.Show(@"Você tem certeza?", @"ACBrNFSe Demo", MessageBoxButtons.YesNo).Equals(DialogResult.No)) return;

                var municipio = ((ACBrMunicipioNFSe)lstMunicipios.SelectedItems[0].Tag).Clone();
                if (FormEdtMunicipio.Editar(municipio).Equals(DialogResult.Cancel)) return;

                AddMunicipio(municipio);
            });
        }

        private void btnDeletar_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                if (lstMunicipios.SelectedItems.Count < 1) return;

                if (MessageBox.Show(@"Você tem certeza?", @"ACBrNFSe Demo", MessageBoxButtons.YesNo).Equals(DialogResult.No)) return;

                var municipio = lstMunicipios.SelectedItems[0];
                lstMunicipios.Items.Remove(municipio);
                UpdateCidades();
            });
        }

        private void btnCarregar_Click(object sender, EventArgs e)
        {
            LoadMunicipios();
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                if (lstMunicipios.Items.Count < 1) return;

                var path = Helpers.SelectFolder();
                if (path.IsEmpty()) return;

                var municipios = lstMunicipios.Items.Cast<ListViewItem>().Select(x => (ACBrMunicipioNFSe)x.Tag);

                ProviderManager.Municipios.Clear();
                ProviderManager.Municipios.AddRange(municipios);
                ProviderManager.Save(Path.Combine(path, "Municipios.nfse"));

                var pathMunicipios = config.Get("CaminhoArqCidades", string.Empty);
                ProviderManager.Save(Path.Combine(pathMunicipios, "Municipios.nfse"));
            });
        }

        private void btnGetCertificate_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                var numeroSerie = acbrNFSe.Configuracoes.Certificados.SelecionarCertificado();
                txtNumeroSerie.Text = numeroSerie;
            });
        }

        private void btnFindCertificate_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                var file = Helpers.OpenFile("Certificate Files (*.pfx)|*.pfx|All Files (*.*)|*.*", "Selecione o certificado");
                txtCertificado.Text = file;
            });
        }

        private void lstMunicipios_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ExecuteSafe(() =>
            {
                if (lstMunicipios.SelectedItems.Count < 1) return;

                var municipio = lstMunicipios.SelectedItems[0].Tag as ACBrMunicipioNFSe;
                if (FormEdtMunicipio.Editar(municipio).Equals(DialogResult.Cancel)) return;

                LoadData();
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            acbrNFSe.Certificado = SelectCertificate(GetCertificatesStore());
            var protocolo = "";
            var numero = 0;
            if (InputBox.Show("Numero protocolo", "Digite o numero do protocolo.", ref protocolo).Equals(DialogResult.Cancel)) return;
            acbrNFSe.Configuracoes.WebServices.Ambiente = cmbAmbiente.GetSelectedValue<DFeTipoAmbiente>();
            acbrNFSe.Configuracoes.PrestadorPadrao.CpfCnpj = txtCPFCNPJ.Text.OnlyNumbers();
            acbrNFSe.Configuracoes.PrestadorPadrao.InscricaoMunicipal = txtIM.Text.OnlyNumbers();
            var ret = acbrNFSe.ConsultarLoteRps(numero, protocolo);
            if (ret.Sucesso)
            {
                this.Log().Info("Sucesso na Consulta! Gravando no repositório de processamento: " + UriRetorno);
                var xmlEnvio = RemoveAllNamespaces(XElement.Parse(ret.XmlRetorno)).ToString();
                ResultadoConsultaLoteRps(protocolo, xmlEnvio, UriRetorno);
            }
            wbbDados.LoadXml(ret.XmlEnvio);
            wbbResposta.LoadXml(ret.XmlRetorno);
        }

        private void preferênciasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Preferencias().Show();
        }

        private void editarConfiguraçãoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var id = ((KeyValuePair<int, string>)cmbConfiguracoes.SelectedItem).Key;
            if (id != 0)
            {
                new FormNova(DBName, id).Show();
            }
        }

        private void txtCPFCNPJ_TextChanged(object sender, EventArgs e)
        {

        }

        #endregion EventHandlers

        #region ValueChanged

        private void txtCNPJ_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.CpfCnpj = txtCPFCNPJ.Text.OnlyNumbers();
        }

        private void txtIM_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.InscricaoMunicipal = txtIM.Text.OnlyNumbers();
        }

        private void txtRazaoSocial_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.RazaoSocial = txtRazaoSocial.Text;
        }

        private void cmbCidades_SelectedValueChanged(object sender, EventArgs e)
        {
            var municipio = (ACBrMunicipioNFSe)cmbCidades.SelectedItem;
            if (municipio == null) return;

            txtUf.Text = municipio.UF.ToString();
            txtCodCidade.Text = municipio.Codigo.ToString();
            txtCodSiafi.Text = municipio.CodigoSiafi.ToString();

            acbrNFSe.Configuracoes.WebServices.CodigoMunicipio = municipio.Codigo;
            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Municipio = municipio.Nome;
            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.CodigoMunicipio = municipio.Codigo;
            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Uf = municipio.UF.ToString();
        }

        private void txtCertificado_TextChanged(object sender, EventArgs e)
        {
            if (txtNumeroSerie.Text.IsEmpty()) return;

            txtNumeroSerie.Text = string.Empty;
            acbrNFSe.Configuracoes.Certificados.Certificado = txtCertificado.Text;
        }

        private void txtSenha_TextChanged(object sender, EventArgs e)
        {
            if (txtSenha.Text.IsEmpty()) return;

            acbrNFSe.Configuracoes.Certificados.Senha = txtSenha.Text;
        }

        private void txtNumeroSerie_TextChanged(object sender, EventArgs e)
        {
            if (txtNumeroSerie.Text.IsEmpty()) return;

            txtCertificado.Text = string.Empty;
            txtSenha.Text = string.Empty;
            acbrNFSe.Configuracoes.Certificados.Certificado = txtNumeroSerie.Text;
            acbrNFSe.Configuracoes.Certificados.Senha = string.Empty;
        }

        private void txtSchemas_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.Arquivos.PathSchemas = config.Get("CaminhoSchemas", string.Empty);
        }

        private void chkSalvarArquivos_CheckedChanged(object sender, EventArgs e)
        {
            //acbrNFSe.Configuracoes.Geral.Salvar = chkSalvarArquivos.Checked;
        }

        private void txtArquivoCidades_Click(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.Arquivos.ArquivoServicos = config.Get("CaminhoArqCidades", string.Empty);
        }

        private void txtPathXml_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.Arquivos.PathSalvar = config.Get("CaminhoNotas",string.Empty);
        }

        private void txtArquivoCidades_TextChanged(object sender, EventArgs e)
        {
        }

        private void cmbAmbiente_SelectedValueChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.WebServices.Ambiente = cmbAmbiente.GetSelectedValue<DFeTipoAmbiente>();
        }

        #endregion ValueChanged

        #region Overrides

        protected override void OnShown(EventArgs e)
        {
            acbrNFSe = new ACBrNFSe();
            acbrNFSe.Configuracoes.Geral.RetirarAcentos = true;

            InitializeLog();
            this.Log().Debug("Log Iniciado");

            cmbAmbiente.EnumDataSource<DFeTipoAmbiente>(DFeTipoAmbiente.Producao);
            LoadData();

            var municipios = lstMunicipios.Items.Cast<ListViewItem>().Select(x => (ACBrMunicipioNFSe)x.Tag);

            ProviderManager.Municipios.Clear();
            ProviderManager.Municipios.AddRange(municipios);

            var pathMunicipios = config.Get("CaminhoArqCidades", string.Empty);
            ProviderManager.Save(Path.Combine(pathMunicipios, "Municipios.nfse"));
        }

        #endregion Overrides

        private void GerarRps(XDocument xml)
        {
            
            if (municipio == null) return;

            acbrNFSe.NotasFiscais.Clear();

            var ns = xml.Root.GetDefaultNamespace();
            var loteRps = xml.Root.Element(ns + "LoteRps");
            var listaRps = loteRps.Element(ns + "ListaRps").Elements(ns + "Rps");
            acbrNFSe.Configuracoes.PrestadorPadrao.CpfCnpj = loteRps.Descendants(ns + "Cnpj").First().Value ?? loteRps.Descendants(ns + "Cpf").First().Value;
            ExecuteSafe(() =>
            {
                foreach (var rps in listaRps)
                {
                    //Recupera a cadeia de informações dos nós principais
                    var nfse          = acbrNFSe.NotasFiscais.AddNew();
                    int codigoMunicipio = acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.CodigoMunicipio;
                    MontaArquivoMunicipio.ChamaPreenchimentoDinamico(codigoMunicipio, rps, ref nfse);

                }
            });
        }
        
        private void AddMunicipio(params ACBrMunicipioNFSe[] municipios)
        {
            ProviderManager.Municipios.AddRange(municipios);
            LoadData();
        }

        private void LoadData()
        {
            var itens = new List<ListViewItem>();

            foreach (var municipio in ProviderManager.Municipios)
            {
                var item = new ListViewItem(municipio.Nome);
                item.SubItems.Add(municipio.UF.ToString());
                item.SubItems.Add(municipio.Codigo.ToString());
                item.SubItems.Add(municipio.CodigoSiafi.ToString());
                item.SubItems.Add(municipio.Provedor.GetDescription());
                item.Tag = municipio;

                itens.Add(item);
            }

            lstMunicipios.BeginUpdate();

            lstMunicipios.Items.Clear();
            lstMunicipios.Items.AddRange(itens.ToArray());

            lstMunicipios.EndUpdate();

            UpdateCidades();
        }

        private void LoadMunicipios(string arquivo = "")
        {
            ExecuteSafe(() =>
            {
                if (arquivo.IsEmpty())
                    arquivo = Helpers.OpenFile("Arquivo de cidades NFSe (*.nfse)|*.nfse*|Todos os arquivos|*.*", "Selecione o arquivo de cidades");
                
                if (arquivo.IsEmpty()) return;

                ProviderManager.Load(arquivo);
                
                LoadData();
            });
        }

        private void UpdateCidades()
        {
            cmbCidades.DataSource = null;
            cmbCidades.Items.Clear();
            cmbCidades.DisplayMember = "Nome";
            cmbCidades.ValueMember = "Codigo";
            cmbCidades.DataSource = ProviderManager.Municipios;
        }

        private void InitializeLog()
        {
            var config = new LoggingConfiguration();
            var target = new RichTextBoxTarget
            {
                UseDefaultRowColoringRules = true,
                Layout = @"${date:format=dd/MM/yyyy HH\:mm\:ss} - ${message}",
                FormName = Name,
                ControlName = rtbLog.Name,
                AutoScroll = true
            };

            config.AddTarget("RichTextBox", target);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));

            var infoTarget = new FileTarget
            {
                FileName = "${basedir:dir=Logs:file=ACBrNFSe.log}",
                Layout = "${processid}|${longdate}|${level:uppercase=true}|" +
                         "${event-context:item=Context}|${logger}|${message}",
                CreateDirs = true,
                Encoding = Encoding.UTF8,
                MaxArchiveFiles = 93,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Date,
                ArchiveFileName = "${basedir}/Logs/Archive/${date:format=yyyy}/${date:format=MM}/ACBrNFSe_{{#}}.log",
                ArchiveDateFormat = "dd.MM.yyyy"
            };

            config.AddTarget("infoFile", infoTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, infoTarget));
            LogManager.Configuration = config;
        }

        private void ExecuteSafe(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                lblStatus.Text += exception.Message +  ". ";
            }
        }

        private string BuscaNumeroProtocolo(XDocument xml)
        {
            var consulta = xml.Root.ElementAnyNs("Protocolo")?.GetValue<string>() ?? string.Empty;

            return consulta;
        }

        private string BuscaNumeroLote(XDocument xml)
        {
            var lote =  xml.ElementAnyNs("NumeroLote").GetValue<string>() ?? xml.Document.Descendants(xml.Root.GetDefaultNamespace() + "NumeroLote").First().Value ?? String.Empty;

            return lote;
        }

        private RetornoWebservice EnviarLoteRpsEnvio(XDocument xxml)
        {

            var numero = int.Parse(BuscaNumeroLote(xxml));
            this.Log().Info($"Enviando lote: {numero}");
            if (numero == 0)
            {
                this.Log().Error("O Número do Lote não foi encontrado!");
                throw new ApplicationException("");
            }
            //if (InputBox.Show("Numero Lote", "Digite o numero do lote.", ref numero).Equals(DialogResult.Cancel)) return;
            GerarRps(xxml);

            var ret = acbrNFSe.Enviar(numero);
                
            wbbDados.LoadXml(ret.XmlEnvio);
            wbbResposta.LoadXml(ret.XmlRetorno);

            return ret;
        }

        private RetornoWebservice ConsultarLoteRps(XDocument xxml, string protocolo = "")
        {

            protocolo = protocolo ?? BuscaNumeroProtocolo(xxml);
            var numero = 0;

            var ret = acbrNFSe.ConsultarLoteRps(numero, protocolo);

            //wbbDados.LoadXml(ret.XmlEnvio);
            //wbbResposta.LoadXml(ret.XmlRetorno);

            return ret;
        }

        private void ResultadoEnvioLoteRps(string lote, string xml, string caminho)
        {
            var param = new System.Collections.Specialized.NameValueCollection
            {
                {"numeroLote", lote},
                {"xml", xml}
            };

            try
            {
                caminho += "?tipoOperacao=1";

                var webCliente = new System.Net.WebClient();
                byte[] bytes_resposta = webCliente.UploadValues(caminho, "POST", param);
                ASCIIEncoding enc = new ASCIIEncoding();
                string resposta = enc.GetString(bytes_resposta);
            }
            catch (System.Net.WebException we)
            {
                this.Log().Error("WebException: Trying to post the rps xml response at URL: '" + caminho + "' - Description: " + we);
            }
            catch (Exception e)
            {
                this.Log().Error("Exception: Trying to post the rps xml response at URL: '" + caminho + "' - Description: " + e);

            }

        }

        private void ResultadoConsultaLoteRps(string protocolo, string xml, string caminho)
        {
            var param = new System.Collections.Specialized.NameValueCollection
            {
                {"protocolo", protocolo},
                {"xml", xml},
                {"tipoOperacao", "2"}
            };

            try
            {
                //caminho += "?tipoOperacao=2";

                var webCliente = new System.Net.WebClient();
                byte[] bytes_resposta = webCliente.UploadValues(caminho, "POST", param);
                string resposta = Encoding.ASCII.GetString(bytes_resposta);
                if (!resposta.IsEmpty() && resposta.ToUpper() != "OK")
                {
                    this.Log().Error("Erro ao gravar Nota Fiscal: \n" + resposta);
                }
            }
            catch (System.Net.WebException we)
            {
                this.Log().Error("WebException: Trying to post the rps xml response at URL: '" + caminho + "' - Description: " + we);
                this.Log().Error(
                    "Oops!! O tempo limite de conexão ao serviço de notificar o estado dos RPSs no Smart expirou. Por favor, verifique sua internet e tente novamente..." +
                    Environment.NewLine + "Erro: " + we.Message, we
                );
            }
            catch (Exception e)
            {
                this.Log().Error("Exception: Trying to post the rps xml response at URL: '" + caminho + "' - Description: " + e);
                this.Log().Error(
                    "Oops!! Ocorreu algum erro ao tentar conectar ao serviço de notificar o estado dos RPSs no Smart. Por favor, verifique sua internet e tente novamente..." +
                    Environment.NewLine + "Erro: " + e.Message, e
                );
            }
        }

        private static XElement RemoveAllNamespaces(XElement e)
        {
            return new XElement(e.Name.LocalName, (from n in e.Nodes()
                                                   select ((n is XElement) ? RemoveAllNamespaces(n as XElement) : n)),
            (e.HasAttributes) ? (from a in e.Attributes() where (!a.IsNamespaceDeclaration)
                                 select new XAttribute(a.Name.LocalName, a.Value)) : null);
        }

        private void ComboBoxConfiguracoes()
        {
            using (var db = new LiteDB.LiteDatabase(DBName))
            {
                Dictionary<int, string> dicionario = new Dictionary<int, string>();
                var prestador = db.GetCollection<ACbr.Net.Storage.Models.Prestador>("prestador").FindAll();
                dicionario.Add(0, "Selecione uma configuração");
                foreach (var p in prestador)
                {
                    dicionario.Add(p.Id, p.NomeConfig);
                }

                cmbConfiguracoes.DataSource = new BindingSource(dicionario, null);
                cmbConfiguracoes.DisplayMember = "Value";
                cmbConfiguracoes.ValueMember = "Key";
            }
        }

        private X509Certificate2Collection GetCertificatesStore()
        {
            X509Store stores = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                stores.Open(OpenFlags.ReadOnly);
                return stores.Certificates;

            }
            catch (Exception e)
            {
                this.Log().Error($"Falha no procedimento de obtenção da lista de certificados internos. Exception: {e.Message}");
                throw;
            }
            finally
            {
                stores.Close();
            }
        }

        private X509Certificate2 SelectCertificate(X509Certificate2Collection store)
        {
            //Procura o certificado único com base no serial armazenado
            X509Certificate2 certificadoValido = null;

            X509Certificate2Collection internalSelecteds = store.Find(X509FindType.FindBySerialNumber, acbrNFSe.Configuracoes.Certificados.Certificado, false);
            //if (internalSelecteds.Count == 1) return internalSelecteds[0];
            
            
            X509Certificate2Collection selecteds = X509Certificate2UI.SelectFromCollection(
            internalSelecteds,
            "Certificados",
            "Selecione um certificado válido",
            X509SelectionFlag.SingleSelection
            );
            foreach (X509Certificate2 cert in selecteds)
            {
                if (cert.SerialNumber == acbrNFSe.Configuracoes.Certificados.Certificado)
                    certificadoValido = cert;
            }
            
            return certificadoValido;
        }
        #endregion Methods

        private void salvarEditarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormNova(DBName).Show();
        }

        private void cmbConfiguracoes_SelectedIndexChanged(object sender, EventArgs e)
        {

            var id = ((KeyValuePair<int, string>)cmbConfiguracoes.SelectedItem).Key;
            if (id != 0)
            {
                using (var db = new LiteDB.LiteDatabase(DBName))
                {
                    var collection = db.GetCollection<ACbr.Net.Storage.Models.Prestador>("prestador");
                    var p = collection.Find(s => s.Id == id).First();

                    txtCPFCNPJ.Text = p.CnpjCpf;
                    txtIM.Text = p.IM;
                    txtRazaoSocial.Text = p.RazaoSocial;
                    var codMunicipio = p.CodCidade;
                    if (codMunicipio > 0)
                    {
                        municipio = ProviderManager.Municipios.SingleOrDefault(x => x.Codigo == codMunicipio);
                        if (municipio != null)
                        {
                            cmbCidades.SelectedItem = municipio;
                            txtUf.Text = municipio.UF.ToString();
                            txtCodSiafi.Text = municipio.CodigoSiafi.ToString();
                            txtCodCidade.Text = municipio.Codigo.ToString();

                            acbrNFSe.Configuracoes.WebServices.CodigoMunicipio = municipio.Codigo;
                            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Municipio = municipio.Nome;
                            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.CodigoMunicipio = municipio.Codigo;
                            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Uf = municipio.UF.ToString();
                        }
                    }

                    switch (p.Ambiente.ToUpper())
                    {
                        case "PRODUÇÃO":
                            cmbAmbiente.SelectedIndex = 0;
                            break;
                        default:
                            cmbAmbiente.SelectedIndex = 1;
                            break;
                    }

                    txtUf.Text = p.Uf;
                    txtCodSiafi.Text = p.CodSiaf.ToString();
                    txtCertificado.Text = p.CertPxsPath;
                    txtNumeroSerie.Text = p.Serial;
                    txtSenha.Text = p.Senha;
                    UriEnvio = p.XmlListaPath;
                    UriRetorno = p.XmlProcessaPath;

                    acbrNFSe.Configuracoes.Certificados.Certificado = p.Serial;
                    acbrNFSe.Configuracoes.PrestadorPadrao.CpfCnpj = p.CnpjCpf;
                    acbrNFSe.Configuracoes.PrestadorPadrao.InscricaoMunicipal = p.IM;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Log().Info("Lendo Arquivos de: " + UriEnvio);
            var url = new UriBuilder(UriEnvio);
            var xml = XDocument.Parse(TWeb.FetchRps(url.Uri));
            acbrNFSe.Configuracoes.WebServices.Ambiente = cmbAmbiente.GetSelectedValue<DFeTipoAmbiente>();
            EnviarLoteRpsEnvio(xml);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ComboBoxConfiguracoes();
        }

    }
}