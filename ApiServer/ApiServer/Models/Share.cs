namespace StyleWerk.NBB.Models;

public record Model_ShareTemplate(Guid TemplateId, ShareTypes Share, Guid? ShareId, ShareRights Rights);
