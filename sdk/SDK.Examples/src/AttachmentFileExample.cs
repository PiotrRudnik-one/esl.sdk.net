using System;
using Silanis.ESL.SDK;
using Silanis.ESL.SDK.Builder;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using Silanis.ESL.SDK.Builder.Internal;

namespace SDK.Examples
{
    public class AttachmentFileExample : SDKSample
    {
        public static void Main(string[] args)
        {
            new AttachmentFileExample ().Run();
        }

        private Stream attachmentInputStream1;

        private Signer signer1;

        public readonly string NAME1 = "Driver's license";
        public readonly string DESCRIPTION1 = "Please upload a scanned copy of your driver's license.";
        public readonly string SIGNER1_ID = "signer1Id";
        public IList<AttachmentRequirement> signer1Attachments;
        private AttachmentRequirement signer1Att1;
        public readonly string ATTACHMENT_FILE_NAME1 = "The attachment1 for signer1.pdf";
        public readonly string ATTACHMENT_FILE_NAME2 = "The attachment1 for signer2.pdf";
        public IList<AttachmentFile> filesAfterUpload;
        public IList<AttachmentFile> filesAfterDelete;
        public EslServerException exception;



        public AttachmentFileExample ()
        {
            this.attachmentInputStream1 = File.OpenRead(new FileInfo(Directory.GetCurrentDirectory() + "/src/document-for-anchor-extraction.pdf").FullName);
     
        }

        override public void Execute()
        {
            // Signer1 with 1 attachment requirement
            signer1 = SignerBuilder.NewSignerWithEmail (email1)
                .WithFirstName ("John")
                    .WithLastName ("Smith")
                    .WithCustomId (SIGNER1_ID)
                    .WithAttachmentRequirement (AttachmentRequirementBuilder.NewAttachmentRequirementWithName (NAME1)
                                               .WithDescription (DESCRIPTION1)
                                               .IsRequiredAttachment ()
                                               .Build ()).Build ();
        
            DocumentPackage superDuperPackage = PackageBuilder.NewPackageNamed(PackageName)
                .DescribedAs("This is a package created using the eSignLive SDK")
                    .WithSigner(signer1)
                    .WithDocument(DocumentBuilder.NewDocumentNamed("test document")
                                  .FromStream(fileStream1, DocumentType.PDF)
                                  .WithSignature(SignatureBuilder.SignatureFor(senderEmail)
                                  .Build())
                                  .Build())
                    .Build();

            packageId = eslClient.CreateAndSendPackage(superDuperPackage);

            retrievedPackage = eslClient.GetPackage (packageId);
            signer1Att1 = retrievedPackage.GetSigner (email1).GetAttachmentRequirement (NAME1);

            byte [] attachment1ForSigner1FileContent = new StreamDocumentSource (attachmentInputStream1).Content ();
            eslClient.UploadAttachment (packageId, signer1Att1.Id, ATTACHMENT_FILE_NAME1,
                    attachment1ForSigner1FileContent, SIGNER1_ID);

            eslClient.UploadAttachment (packageId, signer1Att1.Id, ATTACHMENT_FILE_NAME2,
                    attachment1ForSigner1FileContent, SIGNER1_ID);


            retrievedPackage = eslClient.GetPackage (packageId);
            signer1Att1 = retrievedPackage.GetSigner (email1).GetAttachmentRequirement (NAME1);

            filesAfterUpload = signer1Att1.Files;

            AttachmentFile attachmentFile = filesAfterUpload[0];

            eslClient.DeleteAttachmentFile (packageId, signer1Att1.Id, attachmentFile.Id, SIGNER1_ID);

            retrievedPackage = eslClient.GetPackage (packageId);
            signer1Att1 = retrievedPackage.GetSigner (email1).GetAttachmentRequirement (NAME1);

            filesAfterDelete = signer1Att1.Files;

            eslClient.SignDocuments (packageId);
            eslClient.PackageService.MarkComplete (packageId);
         
            try 
            {
                eslClient.DeleteAttachmentFile (packageId, signer1Att1.Id, filesAfterUpload[1].Id, SIGNER1_ID);
            } 
            catch (EslServerException excp) 
            {
                exception = excp;
            }

        }

    }
}
