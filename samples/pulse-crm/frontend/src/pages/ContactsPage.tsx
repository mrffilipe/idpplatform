import { useEffect, useState, type FormEvent } from 'react'
import { createContact, deleteContact, listContacts, updateContact } from '../services/crmApi'
import type { Contact } from '../types/crm'

export function ContactsPage() {
  const [contacts, setContacts] = useState<Contact[]>([])
  const [error, setError] = useState<string | null>(null)
  const [editing, setEditing] = useState<Contact | null>(null)
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')

  async function load() {
    try {
      setContacts(await listContacts())
      setError(null)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load contacts')
    }
  }

  useEffect(() => {
    void load()
  }, [])

  function openCreate() {
    setEditing(null)
    setName('')
    setEmail('')
    setPhone('')
  }

  function openEdit(c: Contact) {
    setEditing(c)
    setName(c.name)
    setEmail(c.email)
    setPhone(c.phone ?? '')
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    try {
      if (editing) {
        await updateContact(editing.id, { name, email, phone: phone || undefined })
      } else {
        await createContact({ name, email, phone: phone || undefined })
      }
      openCreate()
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Save failed')
    }
  }

  async function handleDelete(id: string) {
    if (!confirm('Excluir contato?')) return
    try {
      await deleteContact(id)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Delete failed')
    }
  }

  return (
    <div className="page">
      <h1>Contatos</h1>
      <p className="muted">CRUD local isolado por tenant (<code>tid</code> no JWT).</p>
      {error && <p className="error">{error}</p>}

      <div className="split">
        <form className="card form" onSubmit={(e) => void handleSubmit(e)}>
          <h2>{editing ? 'Editar' : 'Novo'} contato</h2>
          <label className="field">
            <span>Nome</span>
            <input value={name} onChange={(e) => setName(e.target.value)} required />
          </label>
          <label className="field">
            <span>Email</span>
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </label>
          <label className="field">
            <span>Telefone</span>
            <input value={phone} onChange={(e) => setPhone(e.target.value)} />
          </label>
          <div className="row">
            <button type="submit" className="btn-primary">
              Salvar
            </button>
            {editing && (
              <button type="button" className="btn-ghost" onClick={openCreate}>
                Cancelar
              </button>
            )}
          </div>
        </form>

        <div className="card table-wrap">
          <table>
            <thead>
              <tr>
                <th>Nome</th>
                <th>Email</th>
                <th>Telefone</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {contacts.map((c) => (
                <tr key={c.id}>
                  <td>{c.name}</td>
                  <td>{c.email}</td>
                  <td>{c.phone ?? '—'}</td>
                  <td className="actions">
                    <button type="button" className="btn-ghost" onClick={() => openEdit(c)}>
                      Editar
                    </button>
                    <button type="button" className="btn-ghost danger" onClick={() => void handleDelete(c.id)}>
                      Excluir
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {contacts.length === 0 && <p className="muted">Nenhum contato ainda.</p>}
        </div>
      </div>
    </div>
  )
}
