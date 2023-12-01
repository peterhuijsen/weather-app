/*
Used for describing a consistent description text across the application.
 */
export default function Description({ children, ...params } : any) {
	return (
		<p className={"text-neutral-800 text-md break-all"} {...params}>
			{children}
		</p>
	)
}