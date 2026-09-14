export const getApiErrorMessage = (
  error: any,
  fallback: string
): string => {
  const data = error?.response?.data;

  if (typeof data?.detail === "string") {
    return data.detail;
  }

  if (typeof data?.message === "string") {
    return data.message;
  }

  if (data?.errors && typeof data.errors === "object") {
    const firstValidationMessage = Object.values(data.errors)
      .flat()
      .find((message) => typeof message === "string");

    if (typeof firstValidationMessage === "string") {
      return firstValidationMessage;
    }
  }

  return fallback;
};
