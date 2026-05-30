using IdPPlatform.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulseCrm.Api.Data;
using PulseCrm.Api.Services;

namespace PulseCrm.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/contacts")]
public sealed class ContactsController : ControllerBase
{
    private readonly IIdPUserContext _user;
    private readonly PulseCrmDbContext _db;

    public ContactsController(IIdPUserContext user, PulseCrmDbContext db)
    {
        _user = user;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var tenantId = await TenantIdResolver.ResolveAsync(_user, _db, cancellationToken);
        if (tenantId is null)
        {
            return BadRequest(new
            {
                message = "Tenant não identificado. Conclua o onboarding ou faça login novamente após o pagamento."
            });
        }

        var contacts = await _db.Contacts
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(contacts);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = await TenantIdResolver.ResolveAsync(_user, _db, cancellationToken);
        if (tenantId is null)
        {
            return BadRequest(new { message = "Tenant não identificado." });
        }

        var contact = await _db.Contacts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return contact is null ? NotFound() : Ok(contact);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ContactBody body,
        CancellationToken cancellationToken)
    {
        var tenantId = await TenantIdResolver.ResolveAsync(_user, _db, cancellationToken);
        if (tenantId is null)
        {
            return BadRequest(new { message = "Tenant não identificado." });
        }

        if (string.IsNullOrWhiteSpace(body.Name) || string.IsNullOrWhiteSpace(body.Email))
        {
            return BadRequest(new { message = "name and email are required." });
        }

        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            Name = body.Name.Trim(),
            Email = body.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(body.Phone) ? null : body.Phone.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Contacts.Add(contact);
        await _db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = contact.Id }, contact);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] ContactBody body,
        CancellationToken cancellationToken)
    {
        var tenantId = await TenantIdResolver.ResolveAsync(_user, _db, cancellationToken);
        if (tenantId is null)
        {
            return BadRequest(new { message = "Tenant não identificado." });
        }

        var contact = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (contact is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(body.Name) || string.IsNullOrWhiteSpace(body.Email))
        {
            return BadRequest(new { message = "name and email are required." });
        }

        contact.Name = body.Name.Trim();
        contact.Email = body.Email.Trim();
        contact.Phone = string.IsNullOrWhiteSpace(body.Phone) ? null : body.Phone.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(contact);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = await TenantIdResolver.ResolveAsync(_user, _db, cancellationToken);
        if (tenantId is null)
        {
            return BadRequest(new { message = "Tenant não identificado." });
        }

        var contact = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (contact is null)
        {
            return NotFound();
        }

        _db.Contacts.Remove(contact);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    public sealed record ContactBody(string Name, string Email, string? Phone);
}
