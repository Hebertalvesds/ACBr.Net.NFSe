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
using System.Xml;
using System.Globalization;

namespace ACBr.Net.NFSe.Demo
{
    public partial class FormMain : Form, IACBrLog
    {
        #region Fields

        private ACBrNFSe acbrNFSe;
        private ACBrConfig config;

        private const bool OBRIGATORIO = true;
        private const bool NAOOBRIGATORIO = false;
        

        #endregion Fields

        #region Constructors

        public FormMain()
        {
            InitializeComponent();
            config = ACBrConfig.CreateOrLoad(Path.Combine(Application.StartupPath, "nfse.config"));
        }

        #endregion Constructors

        #region Methods

        #region EventHandlers

        private void btnIniciarProc_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "";
            //Faz a leitura do arquivo de Envio ou Consulta de Lotes e Decide qual processo irá chamar
            if (VerificaTextBox(UriEnvio))
                MessageBox.Show(this, "Caminho do XML de Envio não pode ser vázio!", "Aviso!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (VerificaTextBox(UriEnvio))
                MessageBox.Show(this, "Caminho do XML de Retorno não pode ser vázio!", "Aviso!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                var uriEnvio = new UriBuilder(UriEnvio.Text);
                var xml = XDocument.Parse(TWeb.FetchRps(uriEnvio.Uri));

                var root = xml.Root;

                if (root.Name.ToString().Contains("EnviarLoteRps")) this.EnviarLoteRpsEnvio(xml);
                if (root.Name.ToString().Contains("ConsultarLoteRps")) this.EnviarLoteRpsEnvio(xml);

            }

        }

        private void btnFolderConfig_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                txtFolderConfig.Text = Helpers.SelectFolder();
            });
        }

        private void btnAbreConfig_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                var lConfig = "";
                var path = txtFolderConfig.Text ?? "C:";
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

            //Verifica se um especifíco será criado
            if (txtFolderConfig.Text.IsEmpty())
            {
                MessageBox.Show("Caminho dos Arquivos de configurações na Aba Geral Ausente!", "Erro ao Salvar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (txtConfigName.Text.IsEmpty())
            {
                MessageBox.Show("Nome da Configuração Ausente!", "Erro ao Salvar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SaveNewConfig();
        }

        private void btnGerarEnviarLoteRps_Click(object sender, EventArgs e)
        {
            
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

        private void btnConsultarLote_Click(object sender, EventArgs e)
        {
            if (VerificaTextBox(UriEnvio))
                MessageBox.Show(this, "Caminho do XML de Envio não pode ser vázio!", "Aviso!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (VerificaTextBox(UriEnvio))
                MessageBox.Show(this, "Caminho do XML de Retorno não pode ser vázio!", "Aviso!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                


            }
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

        private void btnSelecionarSchema_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                txtSchemas.Text = Helpers.SelectFolder();
            });
        }

        private void btnPathXml_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                txtPathXml.Text = Helpers.SelectFolder();
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

        private void txtFantasia_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.NomeFantasia = txtFantasia.Text;
        }

        private void txtFone_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.DadosContato.Telefone = txtFone.Text;
        }

        private void txtCEP_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Cep = txtCEP.Text;
        }

        private void txtEndereco_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Logradouro = txtEndereco.Text;
        }

        private void txtNumero_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Numero = txtNumero.Text;
        }

        private void txtComplemento_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Complemento = txtComplemento.Text;
        }

        private void txtBairro_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Bairro = txtBairro.Text;
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
            acbrNFSe.Configuracoes.Arquivos.PathSchemas = txtSchemas.Text;
        }

        private void chkSalvarArquivos_CheckedChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.Geral.Salvar = chkSalvarArquivos.Checked;
        }

        private void txtArquivoCidades_Click(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.Arquivos.ArquivoServicos = txtArquivoCidades.Text;
        }

        private void txtPathXml_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.Arquivos.PathSalvar = txtPathXml.Text;
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
                txtArquivoCidades.Text = arquivo;
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
            var cidades = config.Get("CaminhoArqCidades", string.Empty);
            txtArquivoCidades.Text = cidades;
            if (!cidades.IsEmpty()) LoadMunicipios(cidades);

            var cnpj = config.Get("PrestadorCPFCNPJ", string.Empty);
            if (!cnpj.IsEmpty())
            {
                txtCPFCNPJ.Text = cnpj.FormataCPFCNPJ();
            }

            txtConfigName.Text = !config.Get("UltimaConfiguracao", String.Empty).IsEmpty() ? config.Get("UltimaConfiguracao", String.Empty) : config.Get("NomeConfig", String.Empty);
            txtFolderConfig.Text = config.Get("CaminhoConfiguracoes", String.Empty);
            txtIM.Text = config.Get("PrestadorIM", string.Empty);
            txtRazaoSocial.Text = config.Get("PrestadorRazaoSocial", string.Empty);
            txtFantasia.Text = config.Get("PrestadorFantasia", string.Empty);
            txtFone.Text = config.Get("PrestadorFone", string.Empty);
            txtCEP.Text = config.Get("PrestadorCEP", string.Empty);
            txtEndereco.Text = config.Get("PrestadorEndereco", string.Empty);
            txtNumero.Text = config.Get("PrestadorNumero", string.Empty);
            txtComplemento.Text = config.Get("PrestadorComplemento", string.Empty);
            txtBairro.Text = config.Get("PrestadorBairro", string.Empty);

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

            UriEnvio.Text = config.Get("UriEnvio", string.Empty);
            UriRetorno.Text = config.Get("UriRetorno", string.Empty);
            
        }

        private void SaveConfig()
        {
            //Salva na Configuração Default
            config.Set("CaminhoConfiguracoes", txtFolderConfig.Text);
            config.Set("UltimaConfiguracao", txtConfigName.Text);

            config.Set("PrestadorCPFCNPJ", txtCPFCNPJ.Text.OnlyNumbers());
            config.Set("PrestadorIM", txtIM.Text.OnlyNumbers());
            config.Set("PrestadorRazaoSocial", txtRazaoSocial.Text);
            config.Set("PrestadorFantasia", txtFantasia.Text);
            config.Set("PrestadorFone", txtFone.Text);
            config.Set("PrestadorCEP", txtCEP.Text);
            config.Set("PrestadorEndereco", txtEndereco.Text);
            config.Set("PrestadorNumero", txtNumero.Text);
            config.Set("PrestadorComplemento", txtComplemento.Text);
            config.Set("PrestadorBairro", txtBairro.Text);

            config.Set("Municipio", txtCodCidade.Text.OnlyNumbers());

            config.Set("Ambiente", cmbAmbiente.GetSelectedValue<DFeTipoAmbiente>());

            config.Set("Certificado", txtCertificado.Text);
            config.Set("Senha", txtSenha.Text);
            config.Set("NumeroSerie", txtNumeroSerie.Text);

            config.Set("CaminhoArqCidades", txtArquivoCidades.Text);
            config.Set("UriEnvio", UriEnvio.Text);
            config.Set("UriRetorno", UriRetorno.Text);
            
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
            var consulta = from p in xml.Root.Elements() select p;

            foreach (var registro in consulta)
            {
                if (registro.ToString().IndexOf("Protocolo") > 0)
                {
                    return registro.Value;
                }
            }

            return String.Empty;
        }

        private string BuscaNumeroLote(XDocument xml)
        {
            var ns = xml.Root.GetDefaultNamespace();
            var id = xml.Root.Element(ns + "LoteRps").Element(ns + "NumeroLote").Value;

            return id;
        }

        private NFSeSimNao converteSimNao(int original)
        {
            return (original == 1) ? NFSeSimNao.Sim : NFSeSimNao.Nao ;
        }

        private void EnviarLoteRpsEnvio(XDocument xxml)
        {
            ExecuteSafe(() =>
            {
                var numero = int.Parse(String.Join("", System.Text.RegularExpressions.Regex.Split(BuscaNumeroLote(xxml), @"[^\d]")));
                GerarRps(xxml);
                //if (InputBox.Show("Numero Lote", "Digite o numero do lote.", ref numero).Equals(DialogResult.Cancel)) return;

                var ret = acbrNFSe.Enviar(numero);
                wbbDados.LoadXml(ret.XmlEnvio);
                wbbResposta.LoadXml(ret.XmlRetorno);
            });
        }

        private void ConsultarLoteRps(XDocument xxml)
        {

            var protocolo = BuscaNumeroProtocolo(xxml);
            var numero = String.Join("", System.Text.RegularExpressions.Regex.Split(BuscaNumeroLote(xxml), @"[^\d]"));

            ExecuteSafe(() =>
            {
                var ret = acbrNFSe.ConsultarLoteRps(numero, protocolo);
                wbbDados.LoadXml(ret.XmlEnvio);
                wbbResposta.LoadXml(ret.XmlRetorno);
            });
        }

        private void SaveNewConfig()
        {

            var specificConfig = ACBrConfig.CreateOrLoad(Path.Combine(txtFolderConfig.Text, txtConfigName.Text + ".config"));

            specificConfig.Set("CaminhoConfiguracoes", txtFolderConfig.Text);
            specificConfig.Set("NomeConfig", txtConfigName.Text);
            specificConfig.Set("PrestadorCPFCNPJ", txtCPFCNPJ.Text.OnlyNumbers());
            specificConfig.Set("PrestadorIM", txtIM.Text.OnlyNumbers());
            specificConfig.Set("PrestadorRazaoSocial", txtRazaoSocial.Text);
            specificConfig.Set("PrestadorFantasia", txtFantasia.Text);
            specificConfig.Set("PrestadorFone", txtFone.Text);
            specificConfig.Set("PrestadorCEP", txtCEP.Text);
            specificConfig.Set("PrestadorEndereco", txtEndereco.Text);
            specificConfig.Set("PrestadorNumero", txtNumero.Text);
            specificConfig.Set("PrestadorComplemento", txtComplemento.Text);
            specificConfig.Set("PrestadorBairro", txtBairro.Text);

            specificConfig.Set("Municipio", txtCodCidade.Text.OnlyNumbers());

            specificConfig.Set("Ambiente", cmbAmbiente.GetSelectedValue<DFeTipoAmbiente>());

            specificConfig.Set("Certificado", txtCertificado.Text);
            specificConfig.Set("Senha", txtSenha.Text);
            specificConfig.Set("NumeroSerie", txtNumeroSerie.Text);

            specificConfig.Set("CaminhoArqCidades", txtArquivoCidades.Text);
            specificConfig.Set("UriEnvio", UriEnvio.Text);
            specificConfig.Set("UriRetorno", UriRetorno.Text);

            specificConfig.Save();

        }

        #endregion Methods
    }
}