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
using ACBr.Net.NFSe.Configuracao;
using System.Xml;
using System.Globalization;
using ACBr.Net.NFSe.Demo.Configuracoes;

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

        #endregion Fields

        #region Constructors

        public FormMain()
        {
            InitializeComponent();
            formPreferencias = new Preferencias();
            var configPath = Application.StartupPath + @"\Configuracoes";
            if (!Directory.Exists(configPath)) formPreferencias.Show();
            config = ACBrConfig.CreateOrLoad(Path.Combine(configPath, "nfse.config"));
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
                this.Log().Info("Lendo Arquivos de: " + UriEnvio);
                var url = new UriBuilder(UriEnvio);
                var xml = XDocument.Parse(TWeb.FetchRps(url.Uri));

                var root = xml.Root;

                if (root.Name.ToString().Contains("EnviarLoteRps")) this.EnviarLoteRpsEnvio(xml);
                else if (root.Name.ToString().Contains("ConsultarLoteRps")) this.ConsultarLoteRps(xml);
                else MessageBox.Show("A operação requisita não pode ser processada. Ausência de Elemento EnviarLoteRps ou ConsultarLoteRps");

            }

        }

        private void btnAbreConfig_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                var lConfig = "";
                var path = config.Get("CaminhoConfiguracoes", string.Empty);
                ExecuteSafe(() =>
                {
                    lConfig = Helpers.OpenFile("Arquivo de Configuração (*.config)|*.config*|Todos os arquivos|*.*", path, "Abrir");
                });

                config = ACBrConfig.CreateOrLoad(lConfig);
                LoadConfig();

            });
        }

        private void btnSalvarConfig_Click(object sender, EventArgs e)
        {
            //Salva o Arquivo de Configuração Default Sempre
            SaveConfig();
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

            cmbAmbiente.EnumDataSource<DFeTipoAmbiente>(DFeTipoAmbiente.Homologacao);
            LoadData();
            LoadConfig();

            var municipios = lstMunicipios.Items.Cast<ListViewItem>().Select(x => (ACBrMunicipioNFSe)x.Tag);

            ProviderManager.Municipios.Clear();
            ProviderManager.Municipios.AddRange(municipios);

            var pathMunicipios = config.Get("CaminhoArqCidades", string.Empty);
            ProviderManager.Save(Path.Combine(pathMunicipios, "Municipios.nfse"));
        }

        #endregion Overrides

        private void GerarRps(XDocument xml)
        {
            
            var municipio = (ACBrMunicipioNFSe)cmbCidades.SelectedItem;

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

        private void LoadConfig()
        {
            UriEnvio = config.Get("UriEnvio", string.Empty);
            UriRetorno = config.Get("UriRetorno", string.Empty);

            var cidades = config.Get("CaminhoArqCidades", string.Empty);
            
            if (!cidades.IsEmpty()) LoadMunicipios(cidades);

            var cnpj = config.Get("PrestadorCPFCNPJ", string.Empty);
            if (!cnpj.IsEmpty())
            {
                txtCPFCNPJ.Text = cnpj.FormataCPFCNPJ();
            }

            txtConfigName.Text = !config.Get("UltimaConfiguracao", String.Empty).IsEmpty() ? config.Get("UltimaConfiguracao", String.Empty) : config.Get("NomeConfig", String.Empty);
            
            txtIM.Text = config.Get("PrestadorIM", string.Empty);
            txtRazaoSocial.Text = config.Get("PrestadorRazaoSocial", string.Empty);

            var codMunicipio = config.Get("Municipio", 0);
            if (codMunicipio > 0)
            {
                var municipio = ProviderManager.Municipios.SingleOrDefault(x => x.Codigo == codMunicipio);
                if (municipio != null)
                {
                    cmbCidades.SelectedItem = municipio;
                }
            }

            cmbAmbiente.SelectedItem = config.Get("Ambiente", DFeTipoAmbiente.Homologacao);

            txtCertificado.Text = config.Get("Certificado", string.Empty);
            txtSenha.Text = config.Get("Senha", string.Empty);
            txtNumeroSerie.Text = config.Get("NumeroSerie", string.Empty);            
        }

        private void SaveConfig()
        {
            //Salva na Configuração Default
            config.Set("UltimaConfiguracao", txtConfigName.Text);

            config.Set("PrestadorCPFCNPJ", txtCPFCNPJ.Text.OnlyNumbers());
            config.Set("PrestadorIM", txtIM.Text.OnlyNumbers());
            config.Set("PrestadorRazaoSocial", txtRazaoSocial.Text);

            config.Set("Municipio", txtCodCidade.Text.OnlyNumbers());

            config.Set("Ambiente", cmbAmbiente.GetSelectedValue<DFeTipoAmbiente>());

            config.Set("Certificado", txtCertificado.Text);
            config.Set("Senha", txtSenha.Text);
            config.Set("NumeroSerie", txtNumeroSerie.Text);

            //config.Set("CaminhoArqCidades", txtArquivoCidades.Text);
            config.Set("UriEnvio", UriEnvio);
            config.Set("UriRetorno", UriRetorno);
            
            config.Save();
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

        private bool VerificaTextBox(TextBox textBox)
        {
            return (textBox.Text == "") ? true : false;
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

        private void EnviarLoteRpsEnvio(XDocument xxml)
        {
            ExecuteSafe(() =>
            {
                
                var numero = int.Parse(BuscaNumeroLote(xxml));
                if (numero == 0)
                {
                    MessageBox.Show("O Número do Lote não foi encontrado!", "Envio cancelado..", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //if (InputBox.Show("Numero Lote", "Digite o numero do lote.", ref numero).Equals(DialogResult.Cancel)) return;
                GerarRps(xxml);

                var ret = acbrNFSe.Enviar(numero);
                if (ret.Sucesso)
                {
                    this.Log().Info("Sucesso no processamento! Gravando no repositório de processamento: " + UriRetorno);
                    var numeroLote = ret.NumeroLote;
                    ResultadoEnvioLoteRps(numeroLote, ret.XmlRetorno, UriRetorno);
                }
                wbbDados.LoadXml(ret.XmlEnvio);
                wbbResposta.LoadXml(ret.XmlRetorno);
            });
        }

        private void ConsultarLoteRps(XDocument xxml)
        {

            var protocolo = BuscaNumeroProtocolo(xxml);
            var numero = 0;

            ExecuteSafe(() =>
            {
                var ret = acbrNFSe.ConsultarLoteRps(numero, protocolo);
                if (ret.Sucesso)
                {
                    this.Log().Info("Sucesso na Consulta! Gravando no repositório de processamento: " + UriRetorno);
                    ResultadoConsultaLoteRps(ret.Protocolo, ret.XmlRetorno, UriRetorno);
                }
                wbbDados.LoadXml(ret.XmlEnvio);
                wbbResposta.LoadXml(ret.XmlRetorno);
            });
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
                throw new ApplicationException(
                    "Oops!! O tempo limite de conexão ao serviço de notificar o estado dos RPSs no Smart expirou. Por favor, verifique sua internet e tente novamente..." +
                    Environment.NewLine + "Erro: " + we.Message, we
                );
            }
            catch (Exception e)
            {
                this.Log().Error("Exception: Trying to post the rps xml response at URL: '" + caminho + "' - Description: " + e);
                throw new ApplicationException(
                    "Oops!! Ocorreu algum erro ao tentar conectar ao serviço de notificar o estado dos RPSs no Smart. Por favor, verifique sua internet e tente novamente..." +
                    Environment.NewLine + "Erro: " + e.Message, e
                );
            }

        }

        private void ResultadoConsultaLoteRps(string protocolo, string xml, string caminho)
        {
            var param = new System.Collections.Specialized.NameValueCollection
            {
                {"protocolo", protocolo},
                {"xml", xml}
            };

            try
            {
                caminho += "?tipoOperacao=2";

                var webCliente = new System.Net.WebClient();
                byte[] bytes_resposta = webCliente.UploadValues(caminho, "POST", param);
                ASCIIEncoding enc = new ASCIIEncoding();
                string resposta = enc.GetString(bytes_resposta);
            }
            catch (System.Net.WebException we)
            {
                this.Log().Error("WebException: Trying to post the rps xml response at URL: '" + caminho + "' - Description: " + we);
                throw new ApplicationException(
                    "Oops!! O tempo limite de conexão ao serviço de notificar o estado dos RPSs no Smart expirou. Por favor, verifique sua internet e tente novamente..." +
                    Environment.NewLine + "Erro: " + we.Message, we
                );
            }
            catch (Exception e)
            {
                this.Log().Error("Exception: Trying to post the rps xml response at URL: '" + caminho + "' - Description: " + e);
                throw new ApplicationException(
                    "Oops!! Ocorreu algum erro ao tentar conectar ao serviço de notificar o estado dos RPSs no Smart. Por favor, verifique sua internet e tente novamente..." +
                    Environment.NewLine + "Erro: " + e.Message, e
                );
            }
        }

        #endregion Methods

        private void salvarEditarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormNova().Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var protocolo = "";
            var numero = 0;
            if (InputBox.Show("Numero protocolo", "Digite o numero do protocolo.", ref protocolo).Equals(DialogResult.Cancel)) return;
            var ret = acbrNFSe.ConsultarLoteRps(numero, protocolo);
            if (ret.Sucesso)
            {
                this.Log().Info("Sucesso na Consulta! Gravando no repositório de processamento: " + UriRetorno);
                ResultadoConsultaLoteRps(ret.Protocolo, ret.XmlRetorno, UriRetorno);
            }
            wbbDados.LoadXml(ret.XmlEnvio);
            wbbResposta.LoadXml(ret.XmlRetorno);
        }

        private void preferênciasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Preferencias().Show();
        }
    }
}