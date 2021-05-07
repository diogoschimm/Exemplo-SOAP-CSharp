using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace SoapSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var requisicao = new RequisicaoSoap();
            var endereco = requisicao.ConsultarCEP();
             
            Console.WriteLine(endereco.Cep);
            Console.WriteLine(endereco.End);
            Console.WriteLine(endereco.Bairro);
            Console.WriteLine(endereco.Cidade);
            Console.WriteLine(endereco.UF);
            Console.WriteLine(endereco.Complemento);

        }
    }

    [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Response
    {
        public ResponseBody Body { get; set; }

        public Result GetResult()
        {
            return Body.ConsultaCEPResponse.@return;
        }
    }

    public class ResponseBody
    {
        [XmlElement(ElementName = "consultaCEPResponse", Namespace = "http://cliente.bean.master.sigep.bsb.correios.com.br/")]
        public ConsultaCEPResponse ConsultaCEPResponse { get; set; }
    }

    public class ConsultaCEPResponse
    {
        [XmlElement(ElementName = "return", Namespace = "")]
        public Result @return { get; set; }
    }
    public class Result
    {
        [XmlElement(ElementName = "bairro", Namespace = "")]
        public string Bairro { get; set; }

        [XmlElement(ElementName = "cep", Namespace = "")]
        public string Cep { get; set; }

        [XmlElement(ElementName = "cidade", Namespace = "")]
        public string Cidade { get; set; }

        [XmlElement(ElementName = "complemento", Namespace = "")]
        public string Complemento { get; set; }

        [XmlElement(ElementName = "end", Namespace = "")]
        public string End { get; set; }

        [XmlElement(ElementName = "uf", Namespace = "")]
        public string UF { get; set; }
    }

    public class RequisicaoSoap
    {
        object Deserialize(string objectData, Type type)
        {
            var serializer = new XmlSerializer(type);
            using var reader = new StringReader(objectData);
            return serializer.Deserialize(reader); 
        }

        public Result ConsultarCEP()
        {
            var soapMessage = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
                                                   xmlns:cli=""http://cliente.bean.master.sigep.bsb.correios.com.br/"">
                                  <soapenv:Header />
                                  <soapenv:Body>
                                    <cli:consultaCEP>
                                      <cep>78005180</cep>
                                    </cli:consultaCEP>
                                   </soapenv:Body>
                                </soapenv:Envelope>";

            var url = "https://apps.correios.com.br/SigepMasterJPA/AtendeClienteService/AtendeCliente";

            var data = Encoding.UTF8.GetBytes(soapMessage);

            var request = WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/xml;charset=utf-8";
            request.ContentLength = data.Length;

            var dataStream = request.GetRequestStream();
            dataStream.Write(data, 0, data.Length);
            dataStream.Close();

            var response = request.GetResponse();
            using var stream = response.GetResponseStream();
            using var sr = new StreamReader(stream);

            var strXmlRetorno =  sr.ReadToEnd();

            var resultado = (Response)Deserialize(strXmlRetorno, typeof(Response));
            return resultado.GetResult();
        }
    }
}
