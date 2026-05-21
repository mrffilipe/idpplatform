import { isAxiosError } from 'axios'
import type { ProblemDetails } from '../types'

export function getApiErrorMessage(error: unknown): string {
  if (isAxiosError<ProblemDetails>(error)) {
    const data = error.response?.data
    if (data?.detail) {
      return data.detail
    }
    if (data?.title) {
      return data.title
    }
    if (error.message) {
      return error.message
    }
  }

  if (error instanceof Error) {
    return error.message
  }

  return 'Ocorreu um erro inesperado.'
}
